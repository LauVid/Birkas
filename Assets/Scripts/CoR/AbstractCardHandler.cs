using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public abstract class AbstractCardHandler : ICardHandler
    {
        private ICardHandler _nextHandler;

        public ICardHandler SetNext(ICardHandler handler)
        {
            this._nextHandler = handler;

            return handler;
        }

        public virtual GameObject Handle(GameObject Player)
        {
            if (_nextHandler != null)
            {
                return _nextHandler.Handle(Player);
            }
            else
            {
                Debug.Log("Nothing Happened?");
                return null;
            }
        }
    }