using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableUserWidget : UserWidget, IBeginDragHandler, IDragHandler, IEndDragHandler{
   public virtual void OnBeginDrag(PointerEventData e){ }

   public virtual void OnDrag(PointerEventData e){ }

   public virtual void OnEndDrag(PointerEventData e){ }
}