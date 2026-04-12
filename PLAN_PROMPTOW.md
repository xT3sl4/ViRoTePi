# Plan Promptow - ViRoTePi Frontend

## Podsumowanie projektu
Aplikacja WPF (.NET 4.7.2) - system zarzadzania paczkami InPost.
Panele: MainWindow (klient), AdminPanel (admin), CourierPanel (kurier).

---

## PROMPT 1: Selfie uzytkownika w prawym gornym rogu MainWindow

### Cel
Poprawic wyswietlanie selfie w MainWindow tak, aby dzialalo identycznie jak w AdminPanel - selfie zawsze widoczne w prawym gornym rogu naglowka, w kolku 48x48px, bez warunkowego ukrywania.

### Pliki do zalaczenia
- `frontend/MainWindow.xaml` (caly plik)
- `frontend/MainWindow.xaml.cs` (caly plik)
- `frontend/AdminPanel.xaml` (linie 17-50 jako wzor naglowka)

### Prompt
```
Popraw wyswietlanie selfie uzytkownika w MainWindow.xaml i MainWindow.xaml.cs.

STAN OBECNY:
- MainWindow ma Ellipse 64x64 z Visibility="Collapsed" i DefaultUserIcon jako fallback
- Selfie ladowane jest warunkowo w metodzie LoadSelfie() - jesli brak pliku, Ellipse jest ukrywany
- Naglowek ma 120px wysokosci i brak tla (przezroczysty)

CEL (wzoruj sie na AdminPanel.xaml linie 17-50):
- Ellipse 48x48 w prawym gornym rogu naglowka, zawsze widoczny
- Jesli uzytkownik jest zalogowany i ma selfie - pokazuj selfie w Ellipse
- Jesli zalogowany bez selfie - pokazuj Images/user.png w tym samym Ellipse (uzyj ImageBrush)
- Jesli gość - pokazuj Images/user.png w Ellipse + przycisk "Zaloguj sie" obok
- Usun DefaultUserIcon (Image) - wszystko przez jeden Ellipse z ImageBrush
- Zachowaj klikniecie na Ellipse -> edycja profilu (User_Click)

Pliki do edycji:
1. frontend/MainWindow.xaml - zmien sekcje naglowka (linie 28-98)
2. frontend/MainWindow.xaml.cs - uproscij LoadSelfie() i ConfigureUIForGuest/LoggedIn
```

---

## PROMPT 2: Naglowek MainWindow w stylu AdminPanel

### Cel
Przerobic naglowek MainWindow (gora okna) aby wygladal jak w AdminPanel - zolte tlo #FFCD00, pomaranczowa obwodka, logo tekstowe.

### Pliki do zalaczenia
- `frontend/MainWindow.xaml` (caly plik - po zmianach z Promptu 1)
- `frontend/AdminPanel.xaml` (linie 17-50 jako wzor)

### Prompt
```
Zmien naglowek MainWindow.xaml aby wygladal jak w AdminPanel.

STAN OBECNY MainWindow:
- Naglowek to StackPanel z Grid, Height="120"
- Brak tla/koloru - przezroczysty
- Logo jako Image (inpost_logo.png)
- Brak obwodki

WZOR (AdminPanel.xaml linie 17-50):
- Border z Background="#FFCD00" i BorderBrush="#FF6200" BorderThickness="0,0,0,4"
- Height="80" (nie 120)
- Logo jako TextBlock "InPost" FontSize="32" FontWeight="Bold" Foreground="#FF6200"
- Podtytul jako TextBlock np. "Panel Klienta" FontSize="28" Foreground="#333333"
- Prawa strona: Ellipse z selfie + przycisk Wyloguj (z Promptu 1)

Zmiany:
1. Zamien StackPanel naglowka na Border z tlem #FFCD00 i obwodka #FF6200
2. Zmniejsz wysokosc do 80px
3. Zamien Image logo na TextBlock "InPost" + "Panel Klienta"
4. Prawa strona: selfie Ellipse 48x48 + Wyloguj/Zaloguj (zachowaj logike z Promptu 1)
5. Zaktualizuj Grid.RowDefinitions - pierwszy wiersz z 120 na 80

Edytuj TYLKO: frontend/MainWindow.xaml
```

---

## PROMPT 3: Menu boczne MainWindow w stylu AdminPanel

### Cel
Poprawic wyglad menu bocznego (lewa kolumna) - dodac stylizacje przyciskow, kolory InPost, zaokraglone rogi.

### Pliki do zalaczenia
- `frontend/MainWindow.xaml` (po zmianach z Promptu 2)

### Prompt
```
Popraw wyglad menu bocznego w MainWindow.xaml (Grid.Row="1" Grid.Column="0", linie 101-113).

STAN OBECNY:
- Proste przyciski z Style="MenuButtonStyle" (LightGray, Black text)
- Brak ikon, zaokraglen, efektow hover

CEL - styl spojny z AdminPanel:
- Tlo lewego panelu: #F5F5F5 (jak AdminPanel Window.Background)
- Przyciski z zaokraglonymi rogami (CornerRadius="8")
- Tlo przyciskow: #FFCD00 (zolte InPost)
- Hover: tlo #FF6200 (pomaranczowe), tekst bialy
- Aktywny/wybrany: tlo #FF6200, tekst bialy, BorderBrush="#FF6200"
- FontSize="16", FontWeight="Bold", Foreground="#333333"
- Kazdy przycisk: Height="50", Margin="5,4"
- Padding="15,0,0,0" (tekst wciagniety od lewej)
- Cursor="Hand"

Zaktualizuj MenuButtonStyle w Window.Resources oraz same przyciski.
Dodaj Click handler do przycisku "Pomoc" - uzyj nazwy Help_Click.

Edytuj TYLKO: frontend/MainWindow.xaml
```

---

## PROMPT 4: Panel glowny (DataGrid, formularze) w stylu AdminPanel

### Cel
Przerobic wyglad DataGrid paczek i formularza wysylki aby pasowaly do stylu AdminPanel.

### Pliki do zalaczenia
- `frontend/MainWindow.xaml` (po zmianach z Promptu 3)
- `frontend/AdminPanel.xaml` (linie 120-177 jako wzor DataGrid, linie 163-176 jako wzor RowStyle)

### Prompt
```
Popraw wyglad paneli glownych w MainWindow.xaml - DataGrid i formularz wysylki.

WZOR DATAGRID (AdminPanel.xaml):
- Border z White background, CornerRadius="8"
- Naglowek sekcji w Border z Background="#FF6200", Padding="15", CornerRadius="8,8,0,0"
- Tytul sekcji: FontSize="18", FontWeight="Bold", Foreground="White"
- DataGrid: BorderThickness="0", GridLinesVisibility="Horizontal", HorizontalGridLinesBrush="#E0E0E0"
- RowStyle: Height="50", hover Background="#FFF8E1", selected Background="#FFE082"
- HeadersVisibility="Column"

ZMIANY W PACKAGESPANEL:
1. Dodaj Border-naglowek z "#FF6200" i tytulem "Twoje Paczki" (bialy tekst)
2. Zastosuj RowStyle z AdminPanel (hover, selected kolory)
3. Usun stary TextBlock "Twoje Paczki" - przenies do naglowka Border

ZMIANY W SENDPACKAGEPANEL:
1. Dodaj Border-naglowek z "#FFCD00" i tytulem "Wyslij paczke" 
2. Przyciski formularza: Background="#FF6200", Foreground="White", FontWeight="Bold", CornerRadius (przez Template)
3. TextBox: BorderBrush="#E0E0E0", Padding="10,8", FontSize="14"
4. Przycisk "Wyslij": Background="#FF6200" zamiast #FAD02A

Edytuj TYLKO: frontend/MainWindow.xaml
```

---

## PROMPT 5: CourierPanel w stylu AdminPanel

### Cel
Ujednolicic wyglad CourierPanel z AdminPanel.

### Pliki do zalaczenia
- `frontend/CourierPanel.xaml` (caly plik)
- `frontend/AdminPanel.xaml` (linie 17-50 jako wzor naglowka)

### Prompt
```
Popraw wyglad CourierPanel.xaml aby byl spojny z AdminPanel.

STAN OBECNY:
- Naglowek: White background, TextBlock "INPOST KURIER" z #FAD02A
- Przycisk Wyloguj z MenuButtonStyle (#FAD02A)
- Lista paczek w prostym Border

CEL (wzor AdminPanel):
1. NAGLOWEK:
   - Border Background="#FFCD00", BorderBrush="#FF6200", BorderThickness="0,0,0,4"
   - TextBlock "InPost" FontSize="32" FontWeight="Bold" Foreground="#FF6200"
   - TextBlock "Panel Kuriera" FontSize="28" Foreground="#333333"
   - Prawy: Ellipse 48x48 + Wyloguj (Background="#FF6200", Foreground="White")

2. LISTA PACZEK:
   - Border z CornerRadius="8", Background="White"
   - Naglowek sekcji: Border Background="#FF6200", CornerRadius="8,8,0,0"
   - Tekst "Paczki do wydania" FontSize="18" FontWeight="Bold" Foreground="White"
   - Przyciski "Wydaj paczke": Background="#FF6200", Foreground="White"
   - Hover na wierszach: Background="#FFF8E1"

3. TLO OKNA: Background="#F5F5F5"

Edytuj TYLKO: frontend/CourierPanel.xaml
```

---

## PROMPT 6: Utworzenie okna HelpWindow z filmikiem

### Cel
Stworzyc nowe okno Pomoc (HelpWindow) z osadzonym odtwarzaczem wideo, otwierane z MainWindow.

### Pliki do UTWORZENIA
- `frontend/HelpWindow.xaml` (NOWY)
- `frontend/HelpWindow.xaml.cs` (NOWY)

### Pliki do EDYCJI
- `frontend/MainWindow.xaml.cs` (dodac handler Help_Click)

### Prompt
```
Stworz nowe okno HelpWindow z odtwarzaczem wideo, otwierane z MainWindow.

UTWORZ PLIK frontend/HelpWindow.xaml:
- Window: Title="Pomoc - InPost", Height="650", Width="900", WindowStartupLocation="CenterOwner"
- Background="#F5F5F5"
- Styl spojny z AdminPanel:
  - Naglowek: Border Background="#FFCD00", BorderBrush="#FF6200", BorderThickness="0,0,0,4", Height="60"
  - TextBlock "InPost" + "Pomoc" w naglowku
- Glowna czesc:
  - Border Background="White", CornerRadius="8", Margin="20"
  - MediaElement x:Name="HelpVideo" z kontrolkami:
    - Play, Pause, Stop jako Button
    - Slider dla pozycji wideo (opcjonalnie)
  - Ponizej wideo: TextBlock z krotkim opisem / instrukcja uzytkowania
  - Styl przyciskow: Background="#FF6200", Foreground="White", FontWeight="Bold"

UTWORZ PLIK frontend/HelpWindow.xaml.cs:
- Konstruktor przyjmuje opcjonalny string videoPath
- Metoda Play_Click: HelpVideo.Play()
- Metoda Pause_Click: HelpVideo.Pause()
- Metoda Stop_Click: HelpVideo.Stop()
- W Loaded: jesli videoPath nie pusty, ustaw HelpVideo.Source = new Uri(videoPath)
- Domyslna sciezka: "Videos/help.mp4" w katalogu aplikacji

EDYTUJ frontend/MainWindow.xaml.cs:
- Dodaj metode Help_Click(object sender, RoutedEventArgs e):
  - var helpWindow = new HelpWindow();
  - helpWindow.Owner = this;
  - helpWindow.ShowDialog();

PAMIETAJ: HelpWindow musi byc dodany do projektu (frontend.csproj powinien go automatycznie wykryc jako WPF).
Utworz tez katalog frontend/Videos/ (puste - uzytkownik doda plik help.mp4).
```

---

## PROMPT 7 (opcjonalny): Dolaczenie handlera Help_Click do XAML

### Cel
Upewnic sie ze przycisk "Pomoc" w MainWindow.xaml ma podpiety Click handler.

### Pliki do zalaczenia
- `frontend/MainWindow.xaml` (po wszystkich zmianach)

### Prompt
```
W MainWindow.xaml znajdz przycisk "Pomoc" w menu bocznym i upewnij sie ze ma Click="Help_Click".

Obecny stan (po Promptie 3 powinien juz miec, ale sprawdz):
<Button Content="Pomoc" ... Click="Help_Click" .../>

Jesli brakuje Click="Help_Click" - dodaj go.
```

---

## Podsumowanie kolejnosci

| # | Prompt | Pliki edytowane | Pliki nowe | Zaleznosci |
|---|--------|----------------|------------|------------|
| 1 | Selfie w MainWindow | MainWindow.xaml, MainWindow.xaml.cs | - | brak |
| 2 | Naglowek MainWindow | MainWindow.xaml | - | po Prompt 1 |
| 3 | Menu boczne MainWindow | MainWindow.xaml | - | po Prompt 2 |
| 4 | DataGrid i formularze | MainWindow.xaml | - | po Prompt 3 |
| 5 | CourierPanel styl | CourierPanel.xaml | - | niezalezny (mozna rownolegle) |
| 6 | Okno Pomoc (HelpWindow) | MainWindow.xaml.cs | HelpWindow.xaml, HelpWindow.xaml.cs | po Prompt 3 (potrzebny Help_Click) |
| 7 | Weryfikacja Pomoc | MainWindow.xaml | - | po Prompt 6 |

### Uwagi
- Prompty 1-4 musza byc wykonywane PO KOLEI (kazdy modyfikuje MainWindow.xaml)
- Prompt 5 mozna wykonac ROWNOLEGLE z 1-4 (edytuje inny plik)
- Prompt 6 wymaga ukonczenia Promptu 3 (handler Help_Click w menu)
- Przed kazdym promptem ZALACZ wymienione pliki w aktualnym stanie
- Po kazdym prompcie PRZETESTUJ kompilacje projektu
- Filmik help.mp4 trzeba recznie wrzucic do frontend/Videos/
