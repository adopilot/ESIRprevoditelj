U 2024 na teritoriji Republike Srpske doći će  do promjene većine fiskalnih uređaja.
Prema povratnoj informaciji podrška OFS.ba novi (trenutno jedini) certificirani uređaji neće razumjeti niti i jedne postojeće komande.
Što će reći  da se mora raditi nova implantacija fiskalnih komadi.
Ovaj projekt ima ideju da bude prevoditelj komandi starih fiskalnih sistema prema novom ESIRu.

Tako ko ne može uraditi prepravku postojećih programa da šalju komande prema novom fiskalnom sistemu mogu koristiti ovaj program koji bi trebao da stoji između
vašeg programa i novog fiskalnog drivera.
 

Kompanija za koju radim koristi većinom HCP (Printere certificirane u Federaciji BiH) printere i mi ćemo sigurno implementirati prevođenje ovog protokola prema novom fiskalnom driveru u OFS.ba.

Uz određenu pomoć, donacije ili kooperaciju radili bi i implantaciju prevođenja Fislink (Mikroelektonika) i Fisplus (Galeb)  komandi.
___
## Tehničke karakterske
Program je baziran na .net8 platformi, pa će raditi samo na Windowsima 10 i novije generacije.
Projekt je sastavljen od dvije cjeline,
* Prva  cjelina  je klasični .Net c# library u kojem su  implantirane komande za driver i servis za prevođenje. 
* Drugi projekt je .Net8 Mauil Blazor projekt koji spaja desktop maui funkcionalisti i blazor web gui. (ovo je projekt za krajnjeg korisnika koji želi da ima aplikaciju)

Teoretski bi će biti vrlo jednostavno napraviti gui (korisnički interfejs) koristeći druge alate pa možda čak i konzolu kako bi podržali Windowse 7 ili Linux os-ove ako uspijemo library projekt podvući pod .net standard 2.0

 

