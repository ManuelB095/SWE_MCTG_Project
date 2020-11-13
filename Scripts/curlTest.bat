@echo off
title Monster Trading Cards Game
echo CURL Testing for Monster Trading Cards Game
echo.

echo ------------------------------------------------------------------
echo 1) List All Messages
echo ------------------------------------------------------------------
curl -X GET http://localhost:8080/messages --header "Content-Type: application/json"
echo.
echo.

echo ------------------------------------------------------------------
echo 2) List First Message
echo ------------------------------------------------------------------
curl -X GET http://localhost:8080/messages/1 --header "Content-Type: application/json"
echo.
echo.

echo ------------------------------------------------------------------
echo 3) GET Fail Cases 
echo -- Bad Request
curl -X GET http://localhost:8080/messages/ --header "Content-Type: application/json"
echo.
echo.
echo -- Bad Request
curl -X GET http://localhost:8080/messages/a --header "Content-Type: application/json"
echo. 
echo.
echo -- File Not Found
curl -X GET http://localhost:8080/messages/9999 --header "Content-Type: application/json"
echo.
echo.
echo -- Method Not Allowed
curl -X PLSGET http://localhost:8080/messages/9999 --header "Content-Type: application/json"
echo.  
echo.

echo ------------------------------------------------------------------
echo 4) Post Message
echo ------------------------------------------------------------------
curl -X POST http://localhost:8080/messages --header "Content-Type: application/json" -d "Message from cURL"
echo.
echo.

echo ------------------------------------------------------------------
echo 5) POST Fail Case -- Bad Request
curl -X POST http://localhost:8080/messages/1 --header "Content-Type: application/json" -d "MeSsAge FrOm cURL"
echo.
echo.


echo ------------------------------------------------------------------
echo 6) Update ( Put ) Message
echo ------------------------------------------------------------------
curl -X PUT http://localhost:8080/messages/1 --header "Content-Type: application/json" -d "Changed message from cURL"
echo.
echo.

echo ------------------------------------------------------------------
echo 7) PUT Fail Case -- Bad Request
curl -X PUT http://localhost:8080/messages --header "Content-Type: application/json" -d "Changed message"
echo.
echo.

echo ------------------------------------------------------------------
echo 8) Delete Message
echo ------------------------------------------------------------------
curl -X DELETE http://localhost:8080/messages/1 --header "Content-Type: application/json"
echo.
echo.

echo ------------------------------------------------------------------
echo 9) DELETE Fail Cases -- Bad Request
curl -X DELETE http://localhost:8080/messages --header "Content-Type: application/json" -d "Changed message"
echo.
echo.
echo ##################################################################

