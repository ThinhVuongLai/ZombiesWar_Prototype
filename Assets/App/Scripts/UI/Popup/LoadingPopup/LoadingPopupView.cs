using UnityEngine;

namespace App.UI
{
    public class LoadingPopupView : CanvasBase
    {
        public override ICanvasPresenter FirstSpawn()
            => new LoadingPopupPresenter(this);
    }
}
