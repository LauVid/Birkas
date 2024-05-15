using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public interface ICardHandler
    {
        ICardHandler SetNext(ICardHandler handler);

        GameObject Handle(GameObject Player);
    }