using System;
using System.Globalization;
using System.Linq;

namespace Autofac.Core.Lifetime
{
	public class MatchingScopeLifetime : IComponentLifetime
	{
		private readonly object[] _tagsToMatch;

		public MatchingScopeLifetime(params object[] lifetimeScopeTagsToMatch)
		{
			if (lifetimeScopeTagsToMatch == null)
			{
				throw new ArgumentNullException("lifetimeScopeTagsToMatch");
			}
			_tagsToMatch = lifetimeScopeTagsToMatch;
		}

		public ISharingLifetimeScope FindScope(ISharingLifetimeScope mostNestedVisibleScope)
		{
			if (mostNestedVisibleScope == null)
			{
				throw new ArgumentNullException("mostNestedVisibleScope");
			}
			for (ISharingLifetimeScope sharingLifetimeScope = mostNestedVisibleScope; sharingLifetimeScope != null; sharingLifetimeScope = sharingLifetimeScope.ParentLifetimeScope)
			{
				if (_tagsToMatch.Contains(sharingLifetimeScope.Tag))
				{
					return sharingLifetimeScope;
				}
			}
			throw new DependencyResolutionException(string.Format(CultureInfo.CurrentCulture, MatchingScopeLifetimeResources.MatchingScopeNotFound, new object[1] { string.Join(", ", _tagsToMatch) }));
		}
	}
}
