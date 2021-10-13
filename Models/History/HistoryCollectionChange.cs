using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;

namespace engenious.Content.Models.History
{
    /// <summary>
    ///     A history item for a change occuring on a collection.
    /// </summary>
    /// <typeparam name="T">The generic type of the collection items.</typeparam>
    public class HistoryCollectionChange<T> : IHistoryItem
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly
            Dictionary<Type, Func<object, NotifyCollectionChangedAction, IList, IList, int, int, IHistoryItem>>
            CachedCreators = new();

        private readonly NotifyCollectionChangedAction _action;
        private readonly IList _oldList, _newList;
        private readonly int _oldStartingIndex, _newStartingIndex;
        private readonly IList<T> _reference;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HistoryCollectionChange{T}"/> class.
        /// </summary>
        /// <param name="reference">The list on which the change occured.</param>
        /// <param name="action">The type of change that occured on the list.</param>
        /// <param name="oldList">The list state before the change.</param>
        /// <param name="newList">The list state after the change.</param>
        /// <param name="oldListStartIndex">The starting index the change occured at.</param>
        /// <param name="newListStartIndex">The starting index the change was applied to.</param>
        /// <exception cref="NotImplementedException">Thrown for actions that are not implemented yet.</exception>
        public HistoryCollectionChange(IList<T> reference, NotifyCollectionChangedAction action, IList oldList,
            IList newList, int oldListStartIndex = -1, int newListStartIndex = -1)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove)
                throw new NotImplementedException(); //TODO: implement


            _action = action;
            _reference = reference;
            _oldList = oldList;
            _newList = newList;
            _oldStartingIndex = oldListStartIndex;
            _newStartingIndex = newListStartIndex;
        }

        /// <inheritdoc />
        public void Undo()
        {
            switch (_action)
            {
                case NotifyCollectionChangedAction.Add:
                    UndoAdd();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UndoRemove();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UndoReplace();
                    break;
                case NotifyCollectionChangedAction.Move:
                    UndoMove();
                    break;
            }
        }

        /// <inheritdoc />
        public void Redo()
        {
            switch (_action)
            {
                case NotifyCollectionChangedAction.Add:
                    RedoAdd();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RedoRemove();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RedoReplace();
                    break;
                case NotifyCollectionChangedAction.Move:
                    RedoMove();
                    break;
            }
        }

        /// <summary>
        ///     Creates an instance of the <see cref="HistoryCollectionChange{T}"/> class depending on a given object.
        /// </summary>
        /// <param name="reference">
        ///     The reference of <see cref="IList{T}"/> to create the the <see cref="HistoryCollectionChange{T}"/> for.
        /// </param>
        /// <param name="args">The collection changed arguments.</param>
        /// <returns>
        ///     The created <see cref="HistoryCollectionChange{T}"/> instance, or <c>null</c> if the creation failed.
        /// </returns>
        public static IHistoryItem? CreateInstance(object reference, NotifyCollectionChangedEventArgs args)
        {
            var type = reference.GetType();
            if (CachedCreators.TryGetValue(type, out var lambda))
                return lambda(reference, args.Action, args.OldItems, args.NewItems, args.OldStartingIndex,
                    args.NewStartingIndex);
            type = type.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>));

            if (type == null)
                return null;
            /* while (() == null)
            {
                type = type.BaseType;
                if (type == null)
                    return null;
            }while (type != null)*/
            var itemType = type.GetGenericArguments().First();
            var collectionType = typeof(IList<>).MakeGenericType(itemType);

            var historyType = typeof(HistoryCollectionChange<>).MakeGenericType(itemType);
            var ci = historyType.GetConstructor(new[]
                                                {
                                                    collectionType, typeof(NotifyCollectionChangedAction),
                                                    typeof(IList), typeof(IList), typeof(int), typeof(int)
                                                });
            if (ci == null)
                return null;

            var collectionParamObj1 = Expression.Parameter(typeof(object));
            var collectionParamObj2 = Expression.Parameter(typeof(IList));
            var collectionParamObj3 = Expression.Parameter(typeof(IList));
            var collectionParam1 = Expression.Convert(collectionParamObj1, collectionType);
            //var collectionParam2 = Expression.Convert(collectionParamObj2,collectionType);
            //var collectionParam3 = Expression.Convert(collectionParamObj3,collectionType);

            var actionParam = Expression.Parameter(typeof(NotifyCollectionChangedAction));
            var oldIndexParam = Expression.Parameter(typeof(int));
            var newIndexParam = Expression.Parameter(typeof(int));

            var call = Expression.New(ci, collectionParam1, actionParam,
                collectionParamObj2, collectionParamObj3, oldIndexParam, newIndexParam);

            lambda = Expression
                .Lambda<Func<object, NotifyCollectionChangedAction, IList, IList, int, int, IHistoryItem>>(call,
                    collectionParamObj1,
                    actionParam,
                    collectionParamObj2,
                    collectionParamObj3,
                    oldIndexParam,
                    newIndexParam
                ).Compile();

            CachedCreators[reference.GetType()] = lambda;

            return lambda(reference, args.Action, args.OldItems, args.NewItems, args.OldStartingIndex,
                args.NewStartingIndex);
        }

        private void UndoAdd()
        {
            var index = _newStartingIndex;
            if (index != -1)
                for (var i = index; i < index + _newList.Count; i++)
                    _reference.RemoveAt(i); //TODO: perhaps delete references directly? or check reference
            /*foreach(var e in _newList)
                if (_reference[index] == e)
                    _reference.RemoveAt(index++);
                else
                    throw new NotSupportedException("dunno");*/
            else
                foreach (var e in _newList.OfType<T>())
                    _reference.Remove(e);
        }

        private void RedoAdd()
        {
            var index = _newStartingIndex;
            if (index != -1)

                foreach (var e in _newList.OfType<T>())
                    _reference.Insert(index++, e);
            else
                foreach (var e in _newList.OfType<T>())
                    _reference.Add(e);
        }

        private void UndoRemove()
        {
            var index = _oldStartingIndex;
            if (index != -1)
                foreach (var e in _oldList.OfType<T>())
                    _reference.Insert(index++, e);
            else
                foreach (var e in _oldList.OfType<T>())
                    _reference.Add(e);
        }

        private void RedoRemove()
        {
            var index = _oldStartingIndex;
            if (index != -1)
                foreach (var e in _oldList.OfType<T>())
                    _reference.RemoveAt(index++); //TODO reference check?
            else
                foreach (var e in _oldList.OfType<T>())
                    _reference.Remove(e);
        }

        private void UndoReplace()
        {
            var index = _newStartingIndex;
            if (index != -1)
                foreach (var e in _oldList.OfType<T>())
                    _reference[index++] = e; //TODO reference check
            else
                for (var i = 0; i < _oldList.Count; i++)
                {
                    var eI = _reference.IndexOf((T)_newList[i]); //TODO wtf?
                    _reference[eI] = (T)_oldList[i];
                }
        }

        private void RedoReplace()
        {
            var index = _newStartingIndex;
            if (index != -1)
                foreach (var e in _newList.OfType<T>())
                    _reference[index++] = e; //TODO reference check
            else
                for (var i = 0; i < _oldList.Count; i++)
                {
                    var eI = _reference.IndexOf((T)_oldList[i]);
                    _reference[eI] = (T)_newList[i];
                }
        }

        private void UndoMove()
        {
            UndoAdd();
            UndoRemove();
        }

        private void RedoMove()
        {
            RedoRemove();
            RedoAdd();
        }
    }
}