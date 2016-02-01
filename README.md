# X-WingMiniatures

Handheld
========

Tabletop
========

"New Game"
> Create session
> show session name
> show players and ship selection

"Start Game"
> wait for players to place all ships at starting positions

> show movement phase active
> wait for players to confirm all movement for ships
> reveal movement patters
> let each ship resolve movement in turn

> show attack phase active
> show active ship firing range
> wait for player to select actions
>



X-Wing miniatures TODO:
	-refactor ***
	-make sure the ships sync() their records at appropriate times
	-test combat system
	-test dice ***
	-integration test
	-how to drag ships around (transform gesture?)
	-program in all of the abilities
	-shields activate when hit
	-flames show when critical damaged

Optional optimization:
	-put more animations to make nicer
	-dial down particles on explosion?
	-put image faces on dice by making sprites with the picutres (alpha transparency .png)
	-make lazors look nicer?
	-make dice textures look nice
	-dont use barrel rolls on millenium falcon (or use dodgebarrelroll)
	-stress tokens dont allow players to choose difficult maneuvers and/or actions (sync isStressed record)

x-wing fun stuff:
make flames go towards model, focus more on model above it in 3d space
play with camera effects, zoom in on ship
have little windows pop up, with pilots, close ups of ships fighting *wakingmars
angle of fire appears when ship on modelp
band surrounding base of ship
feedback when putting ship down on screen (voice feedback, scanner comes out, show shield animation, use animation manager for scanning)


rule:
AdvanceGameState() finishes up what is supposed to hapen in the phase and advances
the cases are what is supposed to happen in that phase
example: right before planning phase is over, all ships move
OR
things that happen at the beginning of the phase get put in the last phase's actins as a "cleanup"


Integration notes:

Collections: 
	Player (default)
	Ships
	Obstacles
	Upgrades (?)

the HH will create the ship record WITH the pilot already configured

how to show things lke focus/evade tokens
does the record change on the unity end automatically or do have to set it manually in record handlers?
how does TT know that all players have chosen their ship and are ready to begin game?
how players choose which ship to attack? (have it so tap TT)