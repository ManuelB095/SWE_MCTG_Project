@echo off

REM --------------------------------------------------
REM Monster Trading Cards Game
REM --------------------------------------------------
title Monster Trading Cards Game
echo CURL Testing for Monster Trading Cards Game
echo.

REM --------------------------------------------------
echo 1) Create Users (Registration)
REM Create User
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}"
echo.
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"ursula\", \"Password\":\"user\"}"
echo.

echo.
echo should fail:
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -X POST http://localhost:10001/users --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"different\"}"
echo. 
echo.

REM --------------------------------------------------
echo 2) Login Users
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"daniel\"}"
echo.
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"altenhof\", \"Password\":\"markus\"}"
echo.
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"admin\",    \"Password\":\"admin\"}"
echo.

echo.
echo should fail:
curl -X POST http://localhost:10001/sessions --header "Content-Type: application/json" -d "{\"Username\":\"kienboec\", \"Password\":\"different\"}"
echo.
echo.

REM --------------------------------------------------
echo 3) create card(done by "admin")
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"OrcWarrior\", \"Type\":\"Orc\", \"Element\":\"Normal\", \"Damage\": 12, \"Price\": 2}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"PsyWizzard\", \"Type\":\"Wizzard\", \"Element\":\"Normal\", \"Damage\": 18, \"Price\": 4}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"RedDragon\", \"Type\":\"Dragon\", \"Element\":\"Fire\", \"Damage\": 26, \"Price\": 5}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"Leviathan\", \"Type\":\"Kraken\", \"Element\":\"Water\", \"Damage\": 34, \"Price\": 10}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"Legolad\", \"Type\":\"FireElve\", \"Element\":\"Fire\", \"Damage\": 16, \"Price\": 4}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"KnightRider\", \"Type\":\"Knight\", \"Element\":\"Normal\", \"Damage\": 12, \"Price\": 2}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"Splash\", \"Type\":\"Spell\", \"Element\":\"Water\", \"Damage\": 18, \"Price\": 4}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"MagicFist\", \"Type\":\"Spell\", \"Element\":\"Normal\", \"Damage\": 12, \"Price\": 3}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"WaterGoblin\", \"Type\":\"Goblin\", \"Element\":\"Water\", \"Damage\": 7, \"Price\": 2}
echo.																																																																																		 				    
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"WaterOrc\", \"Type\":\"Orc\", \"Element\":\"Water\", \"Damage\": 14, \"Price\": 3}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"FireWizzard\", \"Type\":\"Wizzard\", \"Element\":\"Fire\", \"Damage\": 15, \"Price\": 3}
echo.
curl -X POST http://localhost:10001/cards --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "{\"Name\":\"BurningHands\", \"Type\":\"Spell\", \"Element\":\"Fire\", \"Damage\": 11, \"Price\": 2}
echo.
echo.

REM --------------------------------------------------
echo 4) create packages (done by "admin")
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "[{\"Id\": 1}, {\"Id\": 2}, {\"Id\": 3}, {\"Id\": 4}, {\"Id\": 5}]
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "[{\"Id\": 6}, {\"Id\": 2}, {\"Id\": 7}, {\"Id\": 8}, {\"Id\": 10}]
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "[{\"Id\": 1}, {\"Id\": 5}, {\"Id\": 9}, {\"Id\": 12}, {\"Id\": 11}]
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "[{\"Id\": 2}, {\"Id\": 4}, {\"Id\": 6}, {\"Id\": 8}, {\"Id\": 10}]
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "[{\"Id\": 4}, {\"Id\": 5}, {\"Id\": 2}, {\"Id\": 3}, {\"Id\": 7}]
echo.
curl -X POST http://localhost:10001/packages --header "Content-Type: application/json" --header "Authorization: admin-mtcgToken" -d "[{\"Id\": 6}, {\"Id\": 9}, {\"Id\": 11}, {\"Id\": 7}, {\"Id\": 10}]
echo.
echo.

REM --------------------------------------------------
echo 5) acquire packages kienboec
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: kienboec-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: altenhof-mtcgToken" -d ""
echo.
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: altenhof-mtcgToken" -d ""
echo.
echo.
echo should fail (no money):
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: kienboec-mtcgToken" -d ""
echo.
echo should fail (no package):
curl -X POST http://localhost:10001/transactions/packages --header "Content-Type: application/json" --header "Authorization: altenhof-mtcgToken" -d ""
echo.
echo.

REM --------------------------------------------------
echo 6) show all acquired cards kienboec
curl -X GET http://localhost:10001/cards --header "Authorization: kienboec-mtcgToken"
echo.
echo should fail (no token)
curl -X GET http://localhost:10001/cards 
echo.
echo.

REM --------------------------------------------------
echo 6) show all acquired cards altenhof
curl -X GET http://localhost:10001/cards --header "Authorization: altenhof-mtcgToken"
echo.
echo should fail (no token)
curl -X GET http://localhost:10001/cards 
echo.
echo.

REM --------------------------------------------------
echo 8) configure deck
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: kienboec-mtcgToken" -d "[{\"Id\": 1}, {\"Id\": 2}, {\"Id\": 3}, {\"Id\": 4}, {\"Id\": 5}]
echo.
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: altenhof-mtcgToken" -d "[{\"Id\": 10}, {\"Id\": 5}, {\"Id\": 1}, {\"Id\": 6}, {\"Id\": 2}]
echo.
echo.
echo should fail and show original from before:
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: altenhof-mtcgToken" -d "[{\"Id\": 3}, {\"Id\": 4}, {\"Id\": 5}, {\"Id\": 6}, {\"Id\": 20}]
echo.
echo.
echo should fail ... only 3 cards set
curl -X PUT http://localhost:10001/deck --header "Content-Type: application/json" --header "Authorization: altenhof-mtcgToken" -d "[{\"Id\": 3}, {\"Id\": 4}, {\"Id\": 5}]
echo.

REM --------------------------------------------------
echo 9) show configured deck 
curl -X GET http://localhost:10001/deck --header "Authorization: kienboec-mtcgToken"
echo.
curl -X GET http://localhost:10001/deck --header "Authorization: altenhof-mtcgToken"
echo.
echo.


REM --------------------------------------------------
echo 10) battle
start /b "kienboec battle" curl -X POST http://localhost:10001/battles --header "Authorization: kienboec-mtcgToken"
start /b "altenhof battle" curl -X POST http://localhost:10001/battles --header "Authorization: altenhof-mtcgToken"
ping localhost -n 20 >NUL 2>NUL

REM --------------------------------------------------
echo 11) stats
curl -X GET http://localhost:10001/stats --header "Authorization: kienboec-mtcgToken"
echo.
curl -X GET http://localhost:10001/stats --header "Authorization: altenhof-mtcgToken"
echo.
echo.