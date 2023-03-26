using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
using System.IO;

namespace Stubber
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Not enough arguments");
                return 1;
            }

            string outputDir = args[args.Length - 1];
            for (int i = 0; i < args.Length - 1; i++)
            {
                string inputPath = args[i];
                var assembly = AssemblyDefinition.ReadAssembly(inputPath);
                notImplementedException = assembly.MainModule.ImportReference(typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
                Strip(assembly);
                var outputPath = outputDir + new FileInfo(inputPath).Name;
                Console.WriteLine("Writing " + outputPath);
                assembly.Write(outputPath, new WriterParameters { WriteSymbols = false });
            }
            return 0;
        }

        static MethodReference notImplementedException;

        static void Strip(AssemblyDefinition assembly)
        {
            // assembly.MainModule.Types.RemoveAll(t => t.IsNotPublic);
            foreach (var type in assembly.MainModule.Types)
                Strip(type);
            StripAttributes(assembly);
        }

        static void Strip(TypeDefinition type)
        {
            StripAttributes(type);

            // Fields
            type.Fields.RemoveAll(field => !field.IsPublic);
            foreach (var field in type.Fields)
                StripAttributes(field);

            // Properties
            foreach (var property in type.Properties)
            {
                if (property.GetMethod != null && !property.GetMethod.IsPublic)
                    property.GetMethod = null;
                if (property.SetMethod != null && !property.SetMethod.IsPublic)
                    property.SetMethod = null;
            }
            type.Properties.RemoveAll(property => property.GetMethod == null && property.SetMethod == null);
            foreach (var property in type.Properties)
                StripAttributes(property);

            // Events
            foreach (var evnt in type.Events)
            {
                if (evnt.AddMethod != null && !evnt.AddMethod.IsPublic)
                    evnt.AddMethod = null;
                if (evnt.RemoveMethod != null && !evnt.RemoveMethod.IsPublic)
                    evnt.RemoveMethod = null;
            }
            type.Events.RemoveAll(evnt => evnt.AddMethod == null && evnt.RemoveMethod == null);
            foreach (var evnt in type.Events)
                StripAttributes(evnt);

            // Methods
            type.Methods.RemoveAll(method => !method.IsPublic);
            foreach (var method in type.Methods)
            {
                Strip(method);
                StripAttributes(method);
            }

            // Nested types
            type.NestedTypes.RemoveAll(nestedType => !nestedType.IsNestedPublic);
            foreach (var nestedType in type.NestedTypes)
                Strip(nestedType);
        }

        static void Strip(MethodDefinition method)
        {
            method.IsInternalCall = false;
            method.Body = new MethodBody(method);
            var ilProcessor = method.Body.GetILProcessor();
            ilProcessor.Emit(OpCodes.Newobj, notImplementedException);
            ilProcessor.Emit(OpCodes.Throw);
        }

        static void StripAttributes(ICustomAttributeProvider obj)
        {
            obj.CustomAttributes.RemoveAll(attribute => attribute.Constructor == null || !attribute.Constructor.Resolve().IsPublic);
        }

        static Collection<T> RemoveAll<T>(this Collection<T> collection, Func<T, bool> predicate)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
                if (predicate(collection[i]))
                    collection.RemoveAt(i);
            return collection;
        }
    }
}
