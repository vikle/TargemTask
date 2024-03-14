# Разлетающиеся кубы: псевдофизическая модель столкновений

1 На сцене присутствуют несколько произвольных конструкций, состоящих из нескольких элементов (кубов). 
Обе конструкции притягиваются к некоторому объекту в сцене таким образом, чтобы гарантированно сталкиваться друг с другом (но не с объектом). 
При столкновении конструкции должны разлетаться на произвольное, но достаточно большое расстояние. 
Также при столкновении элементы конструкций окрашиваются в рандомный цвет.

2 Для оптимизации рендеринга используется собственная реализация GPU Instancing, позволяющая отрисовать несколько объектов за один проход.

3 Интерфейс пользователя состоит из таймера, счетчика столкновений и кнопки.
- Таймер отсчитывает время от нуля, в секундах.
- Счетчик столкновений учитывает только столкновения конструкций в целом, т.е. если при столкновении соприкоснулось несколько элементов конструкций, счетчик должен увеличиается лишь на единицу.
- Кнопка сброса таймера и счетчика в исходное значение.

![image](https://github.com/vikle/TargemTask/assets/11353069/5d073b6b-71f3-42a5-b8e6-50fc21f10bed)
