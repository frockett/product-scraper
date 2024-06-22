

sleep $(( RANDOM % 600 ))

# Checks if the application is already running.
if pgrep -x "product-scraper" > /dev/null
then
    echo "The application is already running."
else
    # If not running, execute the application.
    cd /app
    dotnet product-scraper.dll
fi