namespace App.HealthBar
{
    public interface IHealthBarView
    {
        void SetFillAmount(float ratio);
        void SetVisible(bool visible);
    }
}
