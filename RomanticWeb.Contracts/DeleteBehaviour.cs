using System;

namespace RomanticWeb
{
    /// <summary>Behavior that should be applied when deleting entities.</summary>
    [Flags]
    public enum DeleteBehaviour
    {
        /// <summary>Default delete behavior set to <see cref="DoNothing" /></summary>
        Default = 0x0000000,

        /// <summary>Nothing special should happen.</summary>
        DoNothing = 0x00000000,

        /// <summary>Delete other blank node entities referenced by the deleted entity.</summary>
        [Obsolete]
        DeleteVolatileChildren = 0x00000001,

        /// <summary>Delete other entities referenced by the deleted entity.</summary>
        DeleteChildren = 0x00000003,

        /// <summary>Remove statements that referenced removed blank node entities.</summary>
        [Obsolete]
        NullifyVolatileChildren = 0x00000010,

        /// <summary>Remove statements that referenced removed entities.</summary>
        NullifyChildren = 0x00000030
    }
}
