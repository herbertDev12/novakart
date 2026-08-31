namespace NovaKart.Domain.Rules
{
    public sealed class BusinessRuleValidationException : Exception
    {
        public BusinessRuleValidationException(IBusinessRule brokenRule)
            : base(brokenRule.Message)
        {
            BrokenRule = brokenRule;
        }

        public IBusinessRule BrokenRule { get; }

        public string Code => BrokenRule.Code;
    }
}
