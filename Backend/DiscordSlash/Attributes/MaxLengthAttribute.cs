using Discord;
using Discord.Interactions;

namespace DexterSlash.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class MaxLengthAttribute : ParameterPreconditionAttribute
    {
        private readonly int _maxLength;

        public MaxLengthAttribute(int maxLength)
        {
            _maxLength = maxLength;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
        {
            if (value is string str)
                if (str.Length > _maxLength)
                    return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError($"Please try to summarise your {parameterInfo.Name} a touch! " +
                $"If you're unable to, try send this in two different messages! " +
                $"The length of this should be under {_maxLength}.");
        }
    }
}
