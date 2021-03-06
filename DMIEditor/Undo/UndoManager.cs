﻿using System.Collections.Generic;

namespace DMIEditor.Undo
{
    /*
     * Handles all Mementos & their corresponding logic
     * 
     */
    public class UndoManager
    {
        private readonly UndoItemQueue _queue = new UndoItemQueue(50);

        public void Undo()
        {
            _queue.GetItem()?.ReverseAction();
        }

        public void RegisterUndoItem(UndoItem item)
        {
            _queue.AddItem(item);
        }

        private class UndoItemQueue
        {
            private UndoItem[] _undoItems;
            public int Index;

            public UndoItemQueue(int size)
            {
                _undoItems = new UndoItem[size];
            }

            public void AddItem(UndoItem item)
            {
                if(Index == _undoItems.Length) moveArrayUpOne();

                _undoItems[Index++] = item;
            }

            public UndoItem GetItem()
            {
                if (Index == 0) return null;
                UndoItem item = _undoItems[Index-1];
                _undoItems[--Index] = null;
                return item;
            }

            private void moveArrayUpOne()
            {
                for (int i = 0; i < _undoItems.Length-1; i++)
                {
                    _undoItems[i] = _undoItems[i + 1];
                }

                _undoItems[^1] = null;
                Index = _undoItems.Length - 1;
            }
        } 
    }
}