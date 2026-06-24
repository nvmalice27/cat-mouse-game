public enum MouseState
{
    Normal          = 0,
    Hungry          = 1,
    Rozochocona     = 2,
    Chcaca          = 3,
    Zrozpaczona     = 4,
    Happy           = 5,
    // Collectible — index = value - 10
    Kochana         = 10,   // 0
    Szczesliwa      = 11,   // 1
    Grobol          = 12,   // 2
    Obrazona        = 13,   // 3
    Wakacyjna       = 14,   // 4
    Zla             = 15,   // 5
    Brudna          = 16,   // 6
    Makapaka        = 17,   // 7
    Pirat           = 18,   // 8
    Niewyspana      = 19,   // 9
    WesolaPoPobudce = 20,   // 10
    Myszkujaca      = 21,   // 11
    Tanczaca        = 22,   // 12
    Pachnaca        = 23,   // 13
    Czonstkowa      = 24,   // 14
    // Bad
    Smutna          = 30,
    Zlowroga        = 31,
    Sciekla         = 32,
    ScieklaII       = 33
}

public enum AlarmType { None = 0, Early = 1, Normal = 2 }
