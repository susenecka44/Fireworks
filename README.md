# 08-Fireworks

**Autor:** Julie Vondráčková

## Přehled
Projekt "08-Fireworks" je simulace navržená pro generování a animaci různých typů efektů ohňostrojů s využitím systémů částic. Zahrnuje různé typy částic, včetně raket a explozí, pro vytvoření živých a dynamických displejů ohňostrojů.

## Definice Částic
Systém definuje abstraktní základní třídu `Particle`, která zapouzdřuje společné vlastnosti a chování částic ohňostrojů, jako jsou pozice, barva, velikost a rychlost. Odvozené třídy, jako jsou `RocketParticle` a `ExplosionParticle`, specializují tuto základní třídu pro reprezentaci specifických typů komponent ohňostrojů, každý s unikátními chováními a vizuálními efekty.

## Simulační Engine
Jádrem projektu je třída `Simulation`, která spravuje životní cyklus všech částic v systému. Je zodpovědná za inicializaci systému částic, simulaci dynamiky částic v čase a zpracování generování nových částic pro nahrazení těch, které vypršely. Simulace podporuje různé vzory explozí (např. sféra, krychle) a dynamicky upravuje vlastnosti částic pro dosažení realistických efektů ohňostrojů.

## Příkazové Argumenty
Projekt explicitně nedefinuje příkazové argumenty. Je navržen tak, aby byl integrovatelný do aplikací, které mohou programově ovládat a zobrazovat simulaci.

## Vstupní Data
Vstupní data jsou dodána v .json souboru, jež byl součástí původního příkladu řešení

## Algoritmus
Simulace následuje tyto kroky pro animaci ohňostrojů:
1. Inicializace simulace s předdefinovaným počtem částic.
2. V každém kroku simulace aktualizuje pozice částic na základě jejich rychlostí a aplikuje stárnutí pro snížení jejich velikosti a vyblednutí jejich barev v čase.
3. Odebrání částic, které dosáhly konce svého životního cyklu.
4. Představení nových částic pro nahrazení těch, které vypršely, včetně spouštění nových raket a generování explozí na konci trajektorií raket.
5. Dynamické úpravy vlastností částic pro simulaci různých vzorů explozí a efektů.

## Extra Práce / Bonusy
Odpalování raketek při stisku mezerníku.


Celkem 3 různé druhy explozí - kulová, krychlová a "splash".


Je to hezky barevné.


Raketky se s časem zpomalují.

## Použití AI
1. [pomoc s typy explozí](https://chat.openai.com/share/0e7d8966-418b-4ad5-96c8-4735d2665210)

2. [psaní dokumentace :)](https://chat.openai.com/share/99b490be-2252-4571-8081-5a29199acc3a)
