using System.Linq.Expressions;
using System.Reflection;

namespace CSUnit.Assertions;

internal static class ExpressionAnalyzer
{
    public static string Decompose(Expression expression)
    {
        var results = new List<string>();
        Visitor(expression, results);
        return string.Join("\n| ", results.Distinct());
    }

    private static void Visitor(Expression node, List<string> results)
    {
        if (node is BinaryExpression binary)
        {
            Visitor(binary.Left, results);
            Visitor(binary.Right, results);
            try 
            { 
                string left = CleanName(binary.Left);
                string right = CleanName(binary.Right);
                results.Add($"{left} [{Evaluate(binary.Left)}] {GetOp(binary.NodeType)} {right} [{Evaluate(binary.Right)}]"); 
            } catch { }
        }
        else if (node is MemberExpression or MethodCallExpression)
        {
            try { results.Add($"{CleanName(node)} = {Evaluate(node)}"); } catch { }
        }
    }

    public static string CleanExpressionString(string s)
    {
        while (s.Contains("<>c__DisplayClass"))
        {
            int start = s.IndexOf("value(");
            if (start == -1) break;
            int dot = s.IndexOf('.', start);
            if (dot == -1) break;
            s = s.Remove(start, dot - start + 1);
        }
        return s.Replace(")", "");
    }

    private static string CleanName(Expression node)
    {
        string s = node.ToString();
        if (s.Contains("<>c__DisplayClass"))
        {
            int lastDot = s.LastIndexOf('.');
            if (lastDot != -1) s = s.Substring(lastDot + 1);
        }
        return s.TrimEnd(')');
    }

    private static string GetOp(ExpressionType type) => type switch
    {
        ExpressionType.Add => "+",
        ExpressionType.Subtract => "-",
        ExpressionType.Multiply => "*",
        ExpressionType.Divide => "/",
        ExpressionType.Equal => "==",
        ExpressionType.NotEqual => "!=",
        ExpressionType.GreaterThan => ">",
        ExpressionType.LessThan => "<",
        ExpressionType.GreaterThanOrEqual => ">=",
        ExpressionType.LessThanOrEqual => "<=",
        _ => type.ToString()
    };

    private static object? Evaluate(Expression node)
    {
        try
        {
            if (node is ConstantExpression constant) return constant.Value;
            return Expression.Lambda(node).Compile().DynamicInvoke();
        }
        catch { return "???"; }
    }
}
