using System.Linq.Expressions;

namespace LazyMapper.Projection;

internal class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly Expression _oldExpr;
    private readonly Expression _newExpr;

    private ExpressionParameterReplacer(Expression oldExpr, Expression newExpr)
    {
        _oldExpr = oldExpr;
        _newExpr = newExpr;
    }

    internal static Expression Replace(Expression body, Expression oldExpr, Expression newExpr)
        => new ExpressionParameterReplacer(oldExpr, newExpr).Visit(body);

    protected override Expression VisitParameter(ParameterExpression node)
        => node == _oldExpr
            ? _newExpr
            : base.VisitParameter(node);
}