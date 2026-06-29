public enum MouseState
{
    Normal           = 0,
    Hungry           = 1,   // potrzeba: głód >= 60
    Rozochocona      = 2,
    Chcaca           = 3,   // potrzeba: uwaga >= 60
    Zrozpaczona      = 4,   // zły stan (łańcuch)
    Heppi            = 5,   // tulanie/całowanie w stanie normalnym

    Zakochana        = 10,  // róża (cutscene)
    Czonstkujaca     = 11,  // nakarmienie głodnej
    Grobol           = 12,  // nakarmienie nie-głodnej
    Obrazona         = 13,  // zła akcja (2-fazowa blokada)
    Paczurowa        = 14,  // bilet lotniczy (cutscene)
    // 15 = zarezerwowane (nieużywane)
    Smrodliwa        = 16,  // potrzeba: brud >= 60
    Makapaka         = 17,  // 3 skarpetki
    Pirat            = 18,  // łóżko (trzyma do akcji)
    Niewyspana       = 19,  // za wczesny budzik
    WesolaPoPobudce  = 20,  // normalny budzik
    Myszkujaca       = 21,  // kuchnia (trzyma do wyjścia)
    Tanczaca         = 22,  // dobra muzyka
    Pachnaca         = 23,  // mycie brudnej
    Czonstkowa       = 24,  // 100 czonstek
    Pumpuzka         = 25,  // tulanie chcącej
    Roztopiona       = 26,  // rozochocona + bumpcorzenie
    Spankowa         = 27,  // poduszka/sen
    Czosnkowa        = 28,  // czosnek
    Krowka          = 29,  // uszka krowy + krowa na mysz po sobie

    Smutna           = 30,  // zły stan — łańcuch poziom 1
    Zlowroga         = 31,  // zły stan — łańcuch poziom 3
    Sciekla          = 32,  // zły stan — łańcuch poziom 4
    ScieklaII        = 33   // game over
}

public enum AlarmType { None = 0, Early = 1, Normal = 2 }
