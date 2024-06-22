# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "product-scraper/product-scraper.csproj"
RUN dotnet build "product-scraper/product-scraper.csproj" -c Release -o /app/build
RUN dotnet publish "product-scraper/product-scraper.csproj" -c Release -o /app/publish

# Use the official .NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Install Node.js and Playwright dependencies
RUN apt-get update && apt-get install -y wget gnupg cron
RUN wget -qO- https://deb.nodesource.com/setup_current.x | bash -
RUN apt-get install -y nodejs

RUN npm install -g playwright
RUN npx playwright install-deps
RUN playwright install

COPY run_app.sh /app/run_app.sh

# Ensure the database folder is available and set up as a volume
COPY product-scraper/database /app/database
COPY product-scraper/logs /app/logs
VOLUME ["/app/database"]
VOLUME ["/app/logs"]

# Set permissions for the bash script
RUN chmod +x /app/run_app.sh

# Set up the cron job
# Set permissions for the bash script
RUN chmod +x /app/run_app.sh

# Set up the cron job
RUN echo "0 0-16 * * * /app/run_app.sh" > /etc/cron.d/product-scraper-cron
RUN chmod 0644 /etc/cron.d/product-scraper-cron
RUN crontab /etc/cron.d/product-scraper-cron

# Start cron and keep the container running
ENTRYPOINT ["sh", "-c", "cron && tail -f /dev/null"]
