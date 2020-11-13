@echo off
title Script to Restore Messages Folder to testing conditions
echo.

curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"
curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"
curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"
curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"
curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"
curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"
curl -X DELETE http://localhost:10001/messages/1 --header "Content-Type: application/json"

echo.
echo.
curl -X POST http://localhost:10001/messages --header "Content-Type: application/json" -d "New Content goes here"
echo.
echo.
curl -X POST http://localhost:10001/messages --header "Content-Type: application/json" -d "New Content goes here"
echo.
echo.
echo ##################################################################

