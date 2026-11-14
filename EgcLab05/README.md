# Tema05 – Mesh procedural și camere FPS / orbită

Proiect bazat pe șablonul laboratorului 05: .NET 8, OpenTK 4.9.4.

## Rulare

În Visual Studio: deschide `EgcLab05.csproj` sau adaugă proiectul la soluție prin **Add → Existing Project**, apoi **Set as Startup Project** și F5.

Din terminal, din folderul `Tema`:

```sh
dotnet run --project EgcLab05/EgcLab05.csproj
```

Pe macOS este necesar GLFW (`brew install glfw`). Program.cs încarcă biblioteca Homebrew doar pe macOS; pe Windows este folosită biblioteca pachetului NuGet.

## Implementare

- P1–P5: perspectivă și FOV, buffer de adâncime, toate fețele cubului, sferă UV, orientare FPS și deplasare normalizată înmulțită cu timpul cadrului.
- P6 facultativ: zbor liber.
- Tema: `OrbitCamera` derivată din `Camera`, țintă, distanță, azimut, elevație, matrice view și descriere în consolă.
- Bonusuri: comutare fără salt al poziției/direcției/proiecției; W A S D mută ținta în plan orizontal.
- P7 facultativ (cilindrul) nu este implementat; metoda goală din șablon nu este folosită în scenă.

`OrbitCamera.Position` calculează poziția prin coordonate sferice, iar `ViewMatrix` privește spre țintă. Elevația este limitată la ±89°, distanța la 0,5–50 unități. La intrarea în orbită, ținta se așază la 5 unități în fața camerei FPS. FOV-ul se păstrează la comutare, inclusiv dacă a fost schimbat cu rotița în FPS. În modul mers, după revenirea din orbită, deplasarea păstrează înălțimea preluată; R restabilește poziția inițială la 1,70.

## Comenzi și verificare manuală

- V: comută FPS ↔ orbită; titlul indică modul activ.
- FPS: mouse pentru privire, W A S D pentru mers, Shift pentru viteză; M eliberează/capturează cursorul.
- G: mers/zbor; în zbor Space urcă, Ctrl stânga coboară.
- Orbită: clic stânga ținut + mouse sau săgeți pentru rotire; W A S D mută ținta, Shift accelerează.
- Rotița: schimbă FOV în FPS și numai distanța în orbită.
- R: resetează camera activă; în orbită ținta revine la cubul central.
- I: afișează parametrii în consolă; H: meniul.
- Z: test de adâncime; C: eliminarea fețelor din spate; F: contur; L: VSync; Esc: ieșire.

Verifică V în ambele sensuri, apoi rotirea și rotița în orbită. Cu I poți confirma că FOV-ul nu se schimbă în orbită și elevația rămâne în limite. Redimensionează fereastra și comută între camere: ambele proiecții trebuie să păstreze proporțiile. Cu C activ, cubul și sfera trebuie să rămână întregi; F arată triangularea.

## Răspunsuri la întrebările laboratorului

- La 800 × 600 și vFOV 60°, hFOV ≈ 75,18°. Lățirea ferestrei schimbă hFOV, nu vFOV.
- Bufferul de adâncime trebuie șters la fiecare cadru; altfel adâncimile cadrului anterior pot bloca fragmentele noi.
- Sfera cu 32 de felii și 16 benzi are 33 × 17 = 561 vârfuri și 32 × 16 × 2 = 1024 triunghiuri. 64 sunt degenerate la poli (arie zero), deci rămân 960 nedegenerate.
- Cusătura dublează pozițiile pentru u=0 și u=1, permițând culori/coordonate UV diferite.
- La pitch ±90°, front și worldUp devin paraleli; produsul vectorial pentru right nu mai poate fi normalizat stabil.
- Normalizarea deplasării împiedică accelerarea pe diagonală; înmulțirea cu dt păstrează viteza pe secundă indiferent de FPS.

## Predare

În branch-ul GitHub `tema05` se încarcă numai folderul `EgcLab05`, fără `bin/`, `obj/` sau soluția întreagă. `.gitignore` exclude fișierele generate. Prima linie din `Program.cs` conține adresa de email pentru Classroom.
