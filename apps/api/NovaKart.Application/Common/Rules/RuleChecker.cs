using NovaKart.Domain.Rules;

namespace NovaKart.Application.Common.Rules;

public static class RuleChecker
{
    public static Result CheckRule(IBusinessRule rule) =>
        rule.IsBroken()
            ? Result.Failure(Error.Conflict(rule.Code, rule.Message))
            : Result.Success();
}
