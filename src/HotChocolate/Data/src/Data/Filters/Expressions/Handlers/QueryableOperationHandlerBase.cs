using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HotChocolate.Data.Filters.Expressions
{
    public abstract class QueryableOperationHandlerBase
        : FilterOperationHandler<QueryableFilterContext, Expression>
    {
        protected QueryableOperationHandlerBase(InputParser inputParser)
        {
            InputParser = inputParser;
        }

        protected InputParser InputParser { get; }

        public override bool TryHandleOperation(
            QueryableFilterContext context,
            IFilterOperationField field,
            ObjectFieldNode node,
            [NotNullWhen(true)] out Expression? result)
        {
            IValueNode value = node.Value;
            var parsedValue = InputParser.ParseLiteral(value, field.Type, field.Name);

            if ((!context.RuntimeTypes.Peek().IsNullable || !CanBeNull) && parsedValue is null)
            {
                context.ReportError(
                    ErrorHelper.CreateNonNullError(field, value, context));

                result = null!;
                return false;
            }

            if (field.Type.IsInstanceOfType(value))
            {
                result = HandleOperation(context, field, value, parsedValue);
                return true;
            }

            throw new InvalidOperationException();
        }

        protected bool CanBeNull { get; set; } = true;

        public abstract Expression HandleOperation(
            QueryableFilterContext context,
            IFilterOperationField field,
            IValueNode value,
            object? parsedValue);
    }
}
