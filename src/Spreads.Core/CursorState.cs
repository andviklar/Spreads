// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace Spreads
{
    public enum CursorState : byte
    {
        /// <summary>
        /// A cursor is either not initialized or disposed. Some (re)initialization work may be needed before moving.
        /// </summary>
        None = 0,

        /// <summary>
        /// A cursor is initialized via GetCursor() call of ISeries and is READY TO MOVE.
        /// The cursor is NOWHERE with this state.
        /// </summary>
        Initialized = 1,

        /// <summary>
        /// A cursor has started moving and is at valid position.
        /// A false move from this state must restore the cursor to its position before the move.
        /// </summary>
        Moving = 2,

        // TODO simple MN after MB is possible. But MB must set state so that
        // MN could move, not MN check every time - this is the hottest path.
        // MB puts cursor at the end of the batch.
        
        /// <summary>
        /// A cursor has started batch moving and is at a valid position.
        /// A false move from this state must restore the cursor to its position before the move.
        /// </summary>
        [Obsolete("TODO BatchMoving in not reimplemented after 0.8 changes, rethink if another state is needed for batches")]
        BatchMoving = 4,
    }
}