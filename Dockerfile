FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Установка зависимостей
RUN apt update && apt install -y unzip wget

# Скачивание и установка ASF
RUN wget https://github.com/JustArchiNET/ArchiSteamFarm/releases/latest/download/ASF-linux-x64.zip && \
    unzip ASF-linux-x64.zip && rm ASF-linux-x64.zip

# Копируем конфиги (создай их в своём репозитории)
COPY config /app/config/

CMD ["/app/ArchiSteamFarm"]
