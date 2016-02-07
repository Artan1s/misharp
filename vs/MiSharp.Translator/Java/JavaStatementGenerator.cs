using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MiSharp.Translator.Abstract;

namespace MiSharp.Translator.Java
{
    public class JavaStatementGenerator : IStatementGenerator
    {
        public ILiteralGenerator LiteralGenerator { get { return new JavaLiteralGenerator(); } }

        public IMethodGenerator MethodGenerator { get { return new JavaMethodGenerator(); } }

        public ITypeReferenceGenerator TypeReferenceGenerator { get { return new JavaTypeReferenceGenerator(); } }

        public ITypeReferenceBuilder TypeReferenceBuilder { get { return new JavaTypeReferenceBuilder(); } }

        public IExpressionGenerator ExpressionGenerator { get { return new JavaExpressionGenerator(); } }

        public string Generate(StatementSyntax statement, SemanticModel semanticModel)
        {
            if (statement is IfStatementSyntax)
            {
                return GenerateIfStatement((statement as IfStatementSyntax), semanticModel);
            }
            if (statement is ForEachStatementSyntax)
            {
                return GenerateForEachStatement((statement as ForEachStatementSyntax), semanticModel);
            }
            if (statement is WhileStatementSyntax)
            {
                return GenerateWhileStatement((statement as WhileStatementSyntax), semanticModel);
            }
            if (statement is LocalDeclarationStatementSyntax)
            {
                return GenerateLocalDeclaration((statement as LocalDeclarationStatementSyntax), semanticModel) + ";";
            }
            if (statement is ExpressionStatementSyntax)
            {
                return ExpressionGenerator.GenerateExpression((statement as ExpressionStatementSyntax).Expression, semanticModel) + ";";
            }
            if (statement is ReturnStatementSyntax)
            {
                var returnValueExpression = (statement as ReturnStatementSyntax).Expression;
                if (returnValueExpression != null)
                {
                    return "return " + ExpressionGenerator.GenerateExpression(returnValueExpression, semanticModel) + ";";
                }
                else
                {
                    return "return;";
                }
            }
            if (statement is ThrowStatementSyntax)
            {
                return "throw " + ExpressionGenerator.GenerateExpression((statement as ThrowStatementSyntax).Expression, semanticModel) + ";";
            }
            throw new NotImplementedException();
        }

        private string GenerateWhileStatement(WhileStatementSyntax whileStatementSyntax, SemanticModel semanticModel)
        {
            string condition = ExpressionGenerator.GenerateExpression(whileStatementSyntax.Condition, semanticModel);

            string jStatements;
            if (!(whileStatementSyntax.Statement is BlockSyntax))
            {
                jStatements = Generate(whileStatementSyntax.Statement, semanticModel);
            }
            else
            {
                jStatements = GenerateBlock(whileStatementSyntax.Statement as BlockSyntax, semanticModel);
            }

            string whileStatement = "while (" + condition + ") {";
            if (!string.IsNullOrEmpty(jStatements))
            {
                whileStatement += "\n";
            }
            whileStatement += jStatements.AddTab();

            whileStatement += "\n}";

            return whileStatement;
        }

        private string GenerateForEachStatement(ForEachStatementSyntax forEachStatementSyntax, SemanticModel semanticModel)
        {
            string variable = forEachStatementSyntax.Identifier.ValueText;
            var symbolInfo = semanticModel.GetSymbolInfo(forEachStatementSyntax.Type);
            string variableTypeText;
            if (symbolInfo.Symbol is INamedTypeSymbol)
            {
                var namedTypeSymbol = symbolInfo.Symbol as INamedTypeSymbol;
                var typeReference = TypeReferenceGenerator.GenerateTypeReference(namedTypeSymbol, semanticModel);
                variableTypeText = typeReference.Text;
            }
            else if (symbolInfo.Symbol is ITypeParameterSymbol)
            {
                variableTypeText = (symbolInfo.Symbol as ITypeParameterSymbol).Name;
            }
            else
            {
                throw new NotImplementedException();
            }
            string iterable = ExpressionGenerator.GenerateExpression(forEachStatementSyntax.Expression, semanticModel);
            string jStatements;
            if (!(forEachStatementSyntax.Statement is BlockSyntax))
            {
                jStatements = Generate(forEachStatementSyntax.Statement, semanticModel);
            }
            else
            {
                jStatements = GenerateBlock(forEachStatementSyntax.Statement as BlockSyntax, semanticModel);
            }
            return "for (" + variableTypeText + " " + variable + " : " + iterable + ") {\n"
                + "    " + jStatements + "\n"
                + "}";
        }

        private string GenerateIfStatement(IfStatementSyntax ifStatementSyntax, SemanticModel semanticModel)
        {
            string condition = ExpressionGenerator.GenerateExpression(ifStatementSyntax.Condition, semanticModel);

            string jStatements;
            if (!(ifStatementSyntax.Statement is BlockSyntax))
            {
                jStatements = Generate(ifStatementSyntax.Statement, semanticModel);
            }
            else
            {
                jStatements = GenerateBlock(ifStatementSyntax.Statement as BlockSyntax, semanticModel);
            }

            string ifStatement = "if (" + condition + ") {";
            if (!string.IsNullOrEmpty(jStatements))
            {
                ifStatement += "\n";
            }
            ifStatement += jStatements.AddTab();

            ifStatement += "\n}";
            if (ifStatementSyntax.Else == null)
            {
                return ifStatement;
            }


            ifStatement += " else ";
            if (ifStatementSyntax.Else.Statement is IfStatementSyntax)
            {
                ifStatement += GenerateIfStatement(ifStatementSyntax.Else.Statement as IfStatementSyntax, semanticModel);
                return ifStatement;
            }
            if (!(ifStatementSyntax.Else.Statement is BlockSyntax))
            {
                throw new NotImplementedException();
            }
            ifStatement += "{";
            string elseStatements = GenerateBlock(ifStatementSyntax.Else.Statement as BlockSyntax, semanticModel);
            if (!string.IsNullOrEmpty(elseStatements))
            {
                ifStatement += "\n";
            }
            ifStatement += elseStatements.AddTab() + "\n}";
            return ifStatement;
        }

        private string GenerateBlock(BlockSyntax blockSyntax, SemanticModel semanticModel)
        {
            var statements = new List<string>();
            foreach (var statement in blockSyntax.Statements)
            {
                string generatedStatement = Generate(statement, semanticModel);
                statements.Add(generatedStatement);
            }
            string jStatements = "";
            foreach (var statement in statements)
            {
                jStatements += "\n" + statement;
            }
            jStatements = jStatements.Trim('\n');
            return jStatements;
        }

        private string GenerateLocalDeclaration(LocalDeclarationStatementSyntax localDeclarationStatementSyntax, SemanticModel semanticModel)
        {
            var declaration = localDeclarationStatementSyntax.Declaration;
            var declarationType = declaration.Type;
            string generatedDeclarationType = TypeReferenceGenerator.GenerateTypeReference(declarationType, semanticModel).Text;
            if (declaration.Variables.Count > 1)
            {
                throw new NotImplementedException();
            }
            var declarationVariable = declaration.Variables[0];
            string variableName = declarationVariable.Identifier.ValueText;

            string valueExpression = ExpressionGenerator.GenerateExpression(declarationVariable.Initializer.Value, semanticModel);
            string generatedDeclaration = generatedDeclarationType + " " + variableName + " = " + valueExpression;

            if (!variableName.StartsWith("___"))
            {
                generatedDeclaration = "final " + generatedDeclaration;
            }
            return generatedDeclaration;
        }


    }
}
