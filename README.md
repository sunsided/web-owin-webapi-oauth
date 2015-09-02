# OWIN / Web API 2 / OAuth

Minimalbeispiel für die Authentifizierung mittels OAuth bei Benutzung der Web API 2 in einer Self-Hosted-Applikation mittels OWIN.

## Bauen unter Mono

Folgende Docker-Kommandos ausführen

	docker build -t owin-test . 
	docker run --rm -it -p 9000:9000 -p 9001:9001 owin-test

wobei `owin-test` ein beliebiger Name für das neue Image ist. Beide Befehle sind gekapselt in die Shellskripte

	./docker-build.sh
	./docker-run.sh

wobei diese den Namen des bauenden Nutzers voranstellen, z.B. `mmayer/owin-test`.

## Fallstricke

### Data Protection API

Der standardmäßig eingestellte Protection Service für Daten beruhit auf der Data Protection API (DPAPI). Da es sich hierbei um eine Windows-API handelt, funktionieren diese Calls nicht unter Linux. Abhilfe schafft das Paket [Owin.Security.AesDataProtectorProvider](https://github.com/i4004/Owin.Security.AesDataProtectorProvider), welches mittels

	app.UseAesDataProtectorProvider(...key...);

eingebunden wird.
