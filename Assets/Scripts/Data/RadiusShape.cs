namespace Stokehold
{
    public enum RadiusShape
    {
        Circle,   // круг: расстояние по прямой <= радиус. Радиус 1 это 4 соседа по сторонам, радиус 2 уже с диагоналями
        Square,   // квадрат: радиус 1 это 8 соседей
        Diamond   // ромб: |dx| + |dy| <= радиус
    }
}
