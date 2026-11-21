# Laboratorul 06 — Animații interactive (A, B și C)

Proiect C# / .NET 8 / OpenTK 4.9.4. În Visual Studio: Add → Existing Project → EgcLab06.csproj, apoi Set as Startup Project. Alternativ: `dotnet run --project EgcLab06.csproj` din acest folder.

Sunt implementate P1–P3, input-ul și saltul încărcat din P4, precum și P5 (ierarhia). Deplasarea din P4 este adaptată pentru mașina sport, conform secțiunii de mai jos. Tab parcurge laboratorul, brațul robotic (A), evitarea obstacolelor (B) și roata panoramică (C). H afișează comenzile, I starea, P pune pauză, [ / ] schimbă viteza timpului, N o resetează, B simulează blocarea de 0,5 secunde.

În scena brațului:
- A/D sau stânga/dreapta: rotația bazei.
- W/S sau sus/jos: umărul, limitat la [-75°, 75°].
- Q/Z: cotul, limitat la [-135°, 20°].
- Space: deschide/închide cleștele cu Tween.
- K: salvează unghiurile curente; Enter: redă în buclă/oprește.
- E: comută Linear/EaseInOut; Backspace: șterge pozele.

Pozele sunt păstrate în memorie pe durata rulării. Redarea include o intrare lină în prima poză și revenirea de la ultima la prima. Bonusul de prindere este implementat: Space închide cleștele pe bila verde, care urmează brațul; deschiderea o eliberează sub gravitație. R reașază bila și brațul în poziția inițială, pregătită pentru prindere. Starea apare în titlu și la I.

Verificare manuală: salvează trei poziții diferite cu K, pornește redarea cu Enter, schimbă easing-ul cu E, apoi testează P, [ / ], B și cleștele. În pauză comenzile brațului sunt ignorate. H și I funcționează în ambele scene.

Răspunsuri la întrebările laboratorului:
- Fără limitarea pasului de timp, gravitația poate produce salturi mari de poziție, penetrarea solului și instabilitate numerică după o blocare.
- WasReleased detectează tranziția din apăsat în eliberat; !IsDown este adevărat în fiecare cadru în care tasta nu este ținută, nu doar la eliberare.
- Dacă pivotul lunii devine copilul planetei, moștenește și rotația proprie și scalarea planetei, schimbând orbita și dimensiunea aparentă a lunii.

Predare: folderul EgcLab06 pe branch-ul tema06, fără bin/ și obj/. Prima linie din Program.cs conține emailul Classroom.

## Mașina sport controlabilă

Mașina înlocuiește cubul jucătorului în scena laboratorului. W/sus accelerează înainte; S/jos frânează și intră în marșarier; A/D sau stânga/dreapta virează în mers. La eliberarea accelerației, mașina încetinește. Roțile din față virează, iar toate roțile se rotesc la deplasare. Space păstrează saltul încărcat; R resetează poziția și viteza. P și scara timpului afectează și conducerea.

Această adaptare cerută ulterior schimbă deplasarea pe axe din P4 în conducere cu direcție. Obiectele demonstrative nu au coliziuni cu mașina; deplasarea este limitată la zona podelei.

## Varianta B — evită obstacolele

A/D sau săgețile stânga/dreapta schimbă banda printr-un Tween EaseOut de 0,16 s. Space declanșează saltul, R începe un joc nou. Obstacolele galbene pot fi sărite, cele violet trebuie ocolite. Coliziunile folosesc AABB; viteza și frecvența obstacolelor cresc în timp. Recordul se păstrează între jocuri pe durata sesiunii.

Bonus implementat: marcajele drumului se deplasează folosind distanța parcursă de lume și se opresc la pauză sau accident.

## Varianta C — roata panoramică

Sus/jos modifică viteza țintă, Space oprește lin roata, iar tastele 1–8 aduc cabina numerotată jos la peron printr-un Tween pe drumul cel mai scurt. Cabinele moștenesc rotația butucului și aplică o contrarotație pentru a rămâne verticale.

Bonus implementat: la accelerare/frânare, cabinele se leagănă printr-un resort amortizat, integrat cu subpași din timpul de joc.

## Verificarea opționalelor

P5 și bonusurile A/B/C sunt implementate. Au trecut 26 de verificări automate ale logicii: transformări ierarhice, prindere/transport/eliberare/cădere/resetare bilă, deplasarea și oprirea lumii runner, schimbare bandă, coliziune AABB, oscilații amortizate, pauză, oprirea roții, poziționarea tuturor celor opt cabine, drumul unghiular scurt și limitarea pasului de timp. Compilarea a reușit fără erori sau avertismente. Aceste verificări nu înlocuiesc verificarea vizuală și interactivă în fereastra OpenGL.
