FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app
RUN mkdir publish
 
COPY ./startup.sh .
RUN chmod 755 /app/startup.sh
 
ENV ASPNETCORE_URLS http://+:80
EXPOSE 80
 
CMD ["sh", "./startup.sh"]
