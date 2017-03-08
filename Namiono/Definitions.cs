using System;

namespace Namiono
{
    public enum Actions
    {
        Add,
        Update,
        Remove,
        None
    }

    public enum Types
    {
        User,
        Group,
        Content,
        Nothing
    }

    public enum Needs
    {
        Site,
        User,
        ShoutCast,
        Nothing
    }

    public enum FSObject
    {
        Directory,
        File
    }
}
