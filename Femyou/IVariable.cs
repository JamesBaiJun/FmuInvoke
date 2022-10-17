using System;

namespace Femyou
{
    public interface IVariable
    {
        string Name { get; }
        string Description { get; }
        /// <summary>
        /// 因果关系
        /// </summary>
        string Causality { get; }
    }
}