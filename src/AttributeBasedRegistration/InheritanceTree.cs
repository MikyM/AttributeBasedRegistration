namespace AttributeBasedRegistration;

/// <summary>
/// Inheritance tree
/// </summary>
[PublicAPI]
public class InheritanceTree
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="ancestors"></param>
    public InheritanceTree(Type node, IEnumerable<InheritanceTree> ancestors)
    {
        Node = node;
        Ancestors = ancestors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Current type node.
    /// </summary>
    public Type Node { get; set; }
    /// <summary>
    /// Ancestors.
    /// </summary>
    public IReadOnlyList<InheritanceTree> Ancestors { get; set; }

    /// <summary>
    /// Checks whether the given type is a direct ancestor.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if the given type is a direct ancestor, otherwise false.</returns>
    public bool IsDirectAncestor(Type type)
        => Ancestors.Any(x =>
            (x.Node.IsGenericType ? x.Node.GetGenericTypeDefinition() : x.Node) ==
            (type.IsGenericType ? type.GetGenericTypeDefinition() : type));
}
