namespace Autofac.Core.Activators.Reflection
{
	public interface IConstructorSelector
	{
		ConstructorParameterBinding SelectConstructorBinding(ConstructorParameterBinding[] constructorBindings);
	}
}
