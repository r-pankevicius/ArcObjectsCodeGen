using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace ArcObjectsCodeGen
{
	internal static class CSharpSyntaxHelpers
	{
		static readonly CodeDomProvider s_CsCodeDomProvider = CodeDomProvider.CreateProvider("CSharp");

		public static string ObjectToLiteral(object input)
		{
			using var writer = new StringWriter();

			s_CsCodeDomProvider.GenerateCodeFromExpression(
				new CodePrimitiveExpression(input),
				writer,
				new CodeGeneratorOptions { IndentString = "\t" });

			var literal = writer.ToString();
			return literal;
		}
	}
}
