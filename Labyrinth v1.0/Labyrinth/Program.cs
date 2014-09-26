using ganz = System.Int32;
using lang = System.Int64;
using zeile = System.String;
using Kode = System.ConsoleKey;
using Konsole = System.Console;
using Uhrzeit = System.DateTime;
using boolesche = System.Boolean;
using Ablesen = System.IO.StreamReader;
using Standfarbe = System.ConsoleColor;
using Speicherung = System.IO.StreamWriter;
//Выше реализована жалкая попытка перевести английский на немецкий,
//но зато довольно неплохая. =)

class Steuerung //Управление
{
    private Modellierung modellierung;
    public Steuerung(Modellierung modellierung)
    {
        this.modellierung = modellierung;
    }
    public void Starten()
    {
        while (!modellierung.Sieg && !modellierung.Mord)
            // Пока не достигнута победа и не убит главный персонаж
        {
            Kode kode = Konsole.ReadKey(true).Key; //Запоминаем нажатую кнопку
            //Из-за ожидания кнопки: нельзя реализовать еще один параметр - время так,
            //как хотелось бы...
            modellierung.Bewegung_Zu_Prüfen(kode); //Проверка возможности хода
        }
    }
}
class Modellierung //Моделирование
{
    #region Initialization
    private Abbildung abbildung;
    private Punkt sieg, mensch; //Позиция точки победы и главного героя
    private ganz[,] feld; //Поле
    private ganz schreiten; //Шаги и в Африке шаги
    private Punkt monster; //Пока монстр один, но в идеале их должно быть больше
    private boolesche mord; //Состояние героя - самый главный показатель
    private boolesche art; //Тип монстра, но в идеале этой переменной не должно быть
    #endregion
    //Дальше ничего интересного
    public boolesche Sieg
    {
        get { return mensch.x == sieg.x && mensch.y == sieg.y; }
    }
    public boolesche Mord
    {
        get { return mord; }
    }
    public ganz Schreiten
    {
        get { return schreiten; }
    }
    //А вот дальше МЕГА-КОНСТРУКТОР, главный входной параметр - структура уровня
    public Modellierung(zeile[] niveau, Abbildung abbildung)
    {
        ganz länge = 0; mord = false; schreiten = 0; //Установки по умолчанию
        //Ищем ширину уровня
        for (ganz i = 0; i < niveau.Length; i++)
            if (länge < niveau[i].Length) länge = niveau[i].Length;
        feld = new ganz[länge, niveau.Length];
        //Отсекаем лишние объекты на поле
        for (ganz i = 0; i < feld.GetLength(1); i++)
            for (ganz j = 0; j < niveau[i].Length; j++)
            {
                if (niveau[i][j] == '2') mensch = new Punkt(j, i);
                else if (niveau[i][j] == '3' || niveau[i][j] == '4')
                {
                    monster = new Punkt(j, i);
                    art = niveau[i][j] == '3';
                }
                else if (niveau[i][j] == '9') sieg = new Punkt(j, i);
                if (niveau[i][j] < '2' || niveau[i][j] == '3' || niveau[i][j] == '4')
                    feld[j, i] = niveau[i][j] - '0';
                else feld[j, i] = 0;
            }
        //Отображаем все-все элементы
        abbildung.Feld_Zu_Zeichnen(feld);
        abbildung.Punkt_Zu_Zeichnen(sieg.x, sieg.y, 9); //Подлежит замене
        abbildung.Punkt_Zu_Zeichnen(mensch.x, mensch.y, 2);
        abbildung.Punkt_Zu_Zeichnen(monster.x, monster.y, art ? 3 : 4); //Подлежит замене
        abbildung.Schritte_Zu_Zeigen(1, feld.GetLength(1), schreiten);
        //Здесь конец графического оформления
        this.abbildung = abbildung;
    }
    public void Erwartung() //Слегка бесполезный метод
        //Его главная функция - эффект движения монстра
    {
        Punkt bewegung = monster;
        Bewegung_Des_Monsters();
        if (!bewegung.Equals(monster))
        {
            lang ticken = Uhrzeit.Now.Ticks; //Точность вычисления средняя - 100 наносекунд
            while ((Uhrzeit.Now.Ticks - ticken) < 3000000) ; //Это здесь не спроста
        }
        if (!mord) Bewegung_Des_Monsters(); //Условие это важно, а то нечто странное происходит
    }
    public void Bewegung_Zu_Prüfen(Kode kode) //Движение
    {
        Punkt schritt = mensch;
        boolesche prüfen = true; //Лишняя переменная: стоит для восприятия
        if (kode == Kode.DownArrow && feld[mensch.x, mensch.y + 1] == 0) schritt.y++;
        else if (kode == Kode.UpArrow && feld[mensch.x, mensch.y - 1] == 0) schritt.y--;
        else if (kode == Kode.LeftArrow && feld[mensch.x - 1, mensch.y] == 0) schritt.x--;
        else if (kode == Kode.RightArrow && feld[mensch.x + 1, mensch.y] == 0) schritt.x++;
        else if (kode != Kode.Spacebar) prüfen = false;
        abbildung.Objekt_Zu_Zeichnen(mensch, schritt, 2);
        mensch = schritt;
        if (prüfen)
        {
            abbildung.Schritte_Zu_Zeigen(1, feld.GetLength(1), ++schreiten);
            Erwartung();
        }
    }
    public void Bewegung_Des_Monsters() //Движение противника, а жаль...
    {
        Punkt schritt = monster;
        //Определяем тип монстра, в идеале такого не должно быть
        if (art) Horizontale(ref schritt.x, ref schritt.y);
        else Vertikale(ref schritt.x, ref schritt.y);
        abbildung.Objekt_Zu_Zeichnen(monster, schritt, art ? 3 : 4); //Временно
        feld[schritt.x, schritt.y] = feld[monster.x, monster.y];
        feld[monster.x, monster.y] = 0;
        monster = schritt;
        if (monster.Equals(mensch)) mord = true; //Хотел через ==, но не знаю пока как
        //от override equals избавится, в принципе работает, однако с warning'ом
        //Да и так короче будет
    }
    private void Horizontale(ref ganz x, ref ganz y) //Сам принцип движения
    {
        //Суть в том, что сначала выравниваемся по горизонтали, а потом уж по вертикали
        if (monster.x > mensch.x && feld[monster.x - 1, monster.y] == 0) x--;
        else if (monster.x < mensch.x && feld[monster.x + 1, monster.y] == 0) x++;
        else if (monster.x == mensch.x)
            if (monster.y < mensch.y && feld[monster.x, monster.y + 1] == 0) y++;
            else if (monster.y > mensch.y && feld[monster.x, monster.y - 1] == 0) y--;
    }
    private void Vertikale(ref ganz x, ref ganz y) //Сам принцип движения
    {
        //Суть в том, что сначала выравниваемся по вертикали, а потом уж по горизонтали
        if (monster.y < mensch.y && feld[monster.x, monster.y + 1] == 0) y++;
        else if (monster.y > mensch.y && feld[monster.x, monster.y - 1] == 0) y--;
        else if (monster.y == mensch.y)
            if (monster.x > mensch.x && feld[monster.x - 1, monster.y] == 0) x--;
            else if (monster.x < mensch.x && feld[monster.x + 1, monster.y] == 0) x++;
    }
}
class Abbildung //Отображение
{
    private const ganz anfang = 1; //Точка отсчета координаты
    private const zeile bild = " ▓☺☼☼"; //Почти вся графическая база - нет одного элемента.
    //Билд. =)
    public void Punkt_Zu_Zeichnen(ganz x, ganz y, ganz gestalt)
    {
        Konsole.SetCursorPosition(x + anfang, y + anfang);
        //Дальше нечто странное, упращать не имеет смысла
        //так как это не законченная картина
        if (gestalt == 1) Konsole.ForegroundColor = Standfarbe.White;
        if (gestalt == 2) Konsole.ForegroundColor = Standfarbe.Yellow;
        if (gestalt == 3) Konsole.ForegroundColor = Standfarbe.Red;
        if (gestalt == 4) Konsole.ForegroundColor = Standfarbe.Magenta;
        if (gestalt == 9)
        {
            //Однако побочный эффект есть: если монстр пройдет через эту точку, то
            //она исчезнет. =(
            Konsole.BackgroundColor = Standfarbe.Blue;
            Konsole.Write(bild[0]);
            Konsole.BackgroundColor = Standfarbe.Black;
        }
        if (gestalt < 5) Konsole.Write(bild[gestalt]);
    }
    public void Feld_Zu_Zeichnen(ganz[,] feld)
    {
        ganz breite = 1; //Аварийка
        Konsole.Clear();
        Konsole.ForegroundColor = Standfarbe.White;
        if (feld.GetLength(0) < 15) breite += 15 - feld.GetLength(0); //Еще одна аварийка
        //Возможно, то, что дальше, можна было б написать проще
        Konsole.SetWindowSize(feld.GetLength(0) + anfang + breite, feld.GetLength(1) + anfang + 3);
        Konsole.SetBufferSize(feld.GetLength(0) + anfang + breite, feld.GetLength(1) + anfang + 3);
        //В самом конце "3" означает выдиление места под графу "Шаги: x..."
        //foreach работает медленее
        for (ganz i = 0; i < feld.GetLength(0); i++)
            for (ganz j = 0; j < feld.GetLength(1); j++)
                Punkt_Zu_Zeichnen(i, j, feld[i, j]);
    }
    public void Objekt_Zu_Zeichnen(Punkt alt, Punkt neu, ganz gestalt)
    {
        Punkt_Zu_Zeichnen(alt.x, alt.y, 0);
        Punkt_Zu_Zeichnen(neu.x, neu.y, gestalt);
    }
    public void Schritte_Zu_Zeigen(ganz x, ganz y, ganz schreiten)
    {
        Konsole.SetCursorPosition(x + anfang, y + anfang + 1); //"1" - это аварийка
        //Все это из-за специфики консоли
        Konsole.ForegroundColor = Standfarbe.White;
        Konsole.WriteLine("Шагов: " + schreiten);
    }
}

class Labyrinth
{
    #region Initialization
    private static zeile name; //Имя война
    private static zeile[] niveau; //Структура раунда, вот только смысл немного скрыт
    private static boolesche fahne; //Бесполезная переменная, но это временно
    private static ganz bruch, grenze; //Смысл описан в других местах, а вообще можна обойтись
    //и без них
    private static ganz[] rekorde = new ganz[999]; //Не качественный расход памяти и только
    #endregion
    #region Formalität
    //Все это можна организовать в классе, но я и так старался не нарушать SMA
    public static void Sprung(zeile mitteilung, ganz x, ganz y) //Сокращение кода
    {
        Konsole.SetCursorPosition(x, y);
        Konsole.Write(mitteilung);
    }
    public static void Druck(zeile mitteilung, Standfarbe farbe) //Другой тип сокращения
    {
        Konsole.ForegroundColor = farbe;
        Konsole.Write(mitteilung);
    }
    private static void Reading(zeile nummer) //Нечто непостижимое
    {
        Ablesen strom = new Ablesen("Niveau " + nummer + ".dat");
        niveau = new zeile[ganz.Parse(strom.ReadLine())]; //В идеале должно быть не такое
        for (ganz i = 0; i < niveau.Length; i++)
            niveau[i] = strom.ReadLine();
        strom.Close();
    }
    private static void Name()
    {
        fahne = true; //Интересно, почему я ее сюда прилепил? =)
        Dimension(136, 4); //Этот метод ниже
        Sprung("Назовись, воин: ", 1, 1);
        name = Konsole.ReadLine();
        Konsole.Clear();
        if (name == "") Name(); //Это ЕДИНСТВЕННАЯ проверка на корректность ввода
        //во всей программе в целом. Хотя нет! Брешу! Есть еще один!
    }
    private static void Leiste() //Нашел более лучшую альтернативу, но она позамудренней будет,
        //так что пусть пока будет это:
    {
        Ablesen strom = new Ablesen("Konfiguration.sys");
        Konsole.ForegroundColor = Standfarbe.White; //Погрешность
        grenze = ganz.Parse(strom.ReadLine());
        ganz breite = ganz.Parse(strom.ReadLine()), höhe = ganz.Parse(strom.ReadLine());
        //Три преобразования должны быть в одном методе, да и записаны они должны быть в строку
        Dimension(breite, höhe);
        while (!strom.EndOfStream) Konsole.WriteLine(strom.ReadLine()); //Проверка кстати была бы
        strom.Close();
        Konsole.ReadKey();
        Konsole.Clear();
    }
    private static void Dimension(ganz breite, ganz höhe) //Сокращение
    {
        Konsole.SetWindowSize(breite, höhe);
        Konsole.SetBufferSize(breite, höhe);
    }
    private static void Instruktion() //МЕГА-БЕЗНАДЕЖНАЯ инструкция
        //Как видно занимает довольно много кода
        //Полезноть приблизительно нулевая
        //Да и разобраться крайне затруднительно
        //Метод Druck НЕ переводит строку!!!
    {
        Dimension(50, 11);
        Druck("\n Суть проста:\n\n ", Standfarbe.Yellow);
        Druck("▓ - это есть непроходимый огород\n ", Standfarbe.White);
        Druck("☺", Standfarbe.Yellow);
        Druck(" - это есть, всеми любимый, Вы\n ", Standfarbe.White);
        Druck("☼", Standfarbe.Red);
        Druck(" - чудо-юдо прыг-прыг по горизонтали\n ", Standfarbe.White);
        Druck("☼", Standfarbe.Magenta);
        Druck(" - чудо-юдо прыг-прыг по вертикали\n ", Standfarbe.White);
        Konsole.BackgroundColor = Standfarbe.Blue;
        Konsole.Write(" ");
        Konsole.BackgroundColor = Standfarbe.Black;
        Druck(" - нечто неисследованое, о~, оно манит меня\n\n ", Standfarbe.White);
        Druck("Управление: ", Standfarbe.Yellow);
        Druck("стрелки + пробел (", Standfarbe.White);
        Druck("авось понадобится", Standfarbe.Yellow);
        Druck(")", Standfarbe.White);
        Konsole.ReadKey();
    }
    private static void Einspielung() //Работа с таблицой рекордов
        //Разбил на две секции для понимания
        //Требует существенной доработки и дополнения
    {
        #region Ladung
        ganz menge = 0; //Хм, лишняя переменная
        zeile zeile = ""; //От нее тоже можна избавится при желании
        zeile[,] tabelle = new zeile[2, 999];
        Ablesen strom_1 = new Ablesen("Rekorde.tab"); //Название хотел другое, но логичней так
        while (!strom_1.EndOfStream)
        {
            zeile = strom_1.ReadLine();
            if (zeile != "") //Вот и еще один "обработчик ошибок", правда, обрезанный
            {
                tabelle[0, menge] = zeile.Substring(4, 4); //Вообще был написан метод нужный,
                //но из-за частых переделок был удален
                tabelle[1, menge] = zeile.Substring(9);
                menge++;
            }
        }
        strom_1.Close();
        #endregion
        #region Ausladung
        Speicherung strom_2 = new Speicherung("Rekorde.tab");
        for (ganz i = 0; i < menge; i++)
            if (i < bruch && rekorde[i] < ganz.Parse(tabelle[0, i]))
                strom_2.WriteLine("{0:000} {1:0000} {2}", i + 1, rekorde[i], name);
            else strom_2.WriteLine("{0:000} {1} {2}", i + 1, tabelle[0, i], tabelle[1, i]);
        //То, что в кавычках - явный недостаток, но зато программа работает быстрее
        strom_2.Close();
        #endregion
    }
    private static void Nächste_Niveau()
        //Предложенный метод, но с доработкой
    {
        Abbildung a = new Abbildung();
        Modellierung m = new Modellierung(niveau, a);
        Steuerung s = new Steuerung(m);
        s.Starten();
        //Точка выхода
        if (!m.Mord)
        {
            rekorde[bruch] = m.Schreiten;
            bruch++; //При кончине героя прежде временно, дает возможность сохранить
            //уже набитый результат.
        }
        else fahne = false;
    }
    private static void Das_Ende() //В любом случае конец таков
    {
        Konsole.Clear(); //Явная недоработка
        Dimension(29, 3);
        Druck("\n Вот он какой - конец мой!!!", Standfarbe.Red);
        Konsole.ReadKey();
    }
    #endregion
    public static void Main()
    {
        Konsole.Title = "Лабиринт";
        Konsole.CursorVisible = false;
        Leiste(); Name(); Instruktion(); //Начальные установки,
        //на мой взгляд, ихнее место не здесь
        for (ganz i = 1; i <= grenze && fahne; i++)
        {
            Reading(zeile.Format("{0:000}", i));
            Nächste_Niveau(); //Должен возращать значение типа bool - это заменитель fahne
        }
        //Эти два метода тоже надо было организовать по другому
        Einspielung();
        Das_Ende();
    }
}

struct Punkt //Стандартная структура описывающая точку
{
    public ganz x, y;
    public Punkt(ganz x, ganz y)
    {
        this.x = x;
        this.y = y;
    }
}
//А вот здесь должна быть структура описывающая массив монстров: местоположение и тип



//Версия 1.1

/* ДОПОЛНЕНИЯ ->
 * Обновление переменных и удаление ненужных (модернизация monster sieg)
 * Новые объекты:
 *  1) Зеленый ядовитый газ - уменшает hp
 *  2) Огонь - уменшает hp (возможно с мерцанием)
 *  3) Летучая мина - новый тип монтра: две вариации движение:
 *   а) По заданым траекториям
 *   б) "До упора"
 *  4) Умножитель aka Плодитель aka Размножитель - объект, который размножается
 *  5) Вода, но это уже ближе к типу ландшафта, который уменшает скорость
 *  6) Спидди - увеличитель скорости
 *  7) Тормоз - уменшитель скорости
 *  8) Штраф - разный негативный еффект
 *  9) Шипы - уменшает hp
 *  10) Фризз - замораживает все мобов
 * Шкала:
 *  1) Здоровья (до 10)
 *  2) Попыток (до 3)
 * Доработка:
 *  1) Проверка правильности входных параметров
 *  2) Убрать перекрытие победных точек
 *  3) Решение проблемы с таблицой рекордов
 *  4) + звук и + музыка
 * Новые типы монстров со своими характеристиками:
 *  1) Прыгун
 *  2) Поджигатель
 *  ...
 * Кнопки: появление и исчезновение преград
 * Телепорты
 * Каниболизм монстров
 * Закрывающиеся двери
 * Приз, который позволяет поставить в произвольном месте блок
 * Добавить почву aka ландшафт
 * Все объекты должны идти массивом
 * Под вопросом: организация перечисления, шифрование выходных данных
 * Главное: отойти от консоли и перейти на win-приложения
 * Увеличить инструкцию
 * Изменить тип стенок: должно быть не сплошняком на всю клетку, а только на ребрах перегородки
 * (так и сложность, и смысл игры изменится)
 * Показатель скорости
 * Анимация
 * Меню
 * Как вариант видео вставки (Да поможет мне Виртуал Даб)
 * Слегка измененная логика игры
 * Больше раундов с повышающай сложностью
 * Если получится, то добится использования DX9 и полноэкранного режима,
 *    что с успехом получилось в VB6.5rus, однако работоспособность под Vista
 *    под вопросом в таком случае
 * 
 * 
 * Как только появятся еще идеи, будет еще дополнение...
 * В версии 1.1, наверное, удастся реализовать только часть дополнений,
 *    остальное увидет свет, наверное, в последующих версиях.
*/