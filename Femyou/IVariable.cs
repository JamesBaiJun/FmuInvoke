using System;

namespace Femyou
{
    public interface IVariable
    {
        string Name { get; }
        string Description { get; }
        /// <summary>
        /// �����ϵ
        /// </summary>
        string Causality { get; }
    }
}