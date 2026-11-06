namespace ImproveYourself.Maui.Legal;

public static class LegalDocumentText
{
    public static string Privacy(string language) => language switch
    {
        "ru" => PrivacyRu,
        "de" => PrivacyDe,
        _ => LegalDocuments.PrivacyPolicyBody,
    };

    public static string Terms(string language) => language switch
    {
        "ru" => TermsRu,
        "de" => TermsDe,
        _ => LegalDocuments.TermsOfServiceBody,
    };

    private const string PrivacyRu = """
Последнее обновление: 25 сентября 2026

Improve Yourself («мы», «приложение») помогает выстраивать ежедневные привычки уверенного общения. Эта политика объясняет, какие данные обрабатывает приложение, зачем и кто ещё их обрабатывает.

1. Кто отвечает
Приложением управляет Дмитрий Чубрик. Вопросы о данных и поддержка: supportimproveyourself@gmail.com.

2. Какие данные обрабатываются
• Данные аккаунта: адрес эл. почты, хеш пароля (сам пароль не хранится), идентификатор аккаунта и токены входа.
• Содержимое приложения: имя, прогресс дней, отметки шагов, ответы самооценки и настройки.
• События использования: ограниченный список событий внутри приложения (например, «день завершён») с версией, платформой и типом устройства. Они отправляются только после входа и связаны с аккаунтом.
• Служебные данные: IP-адрес, время запроса, маршрут, статус ответа и диагностика ошибок в журналах сервера.
• Локальное хранение: прогресс и настройки лежат на устройстве; токены доступа хранятся в защищённом хранилище системы.

Напоминания создаются на устройстве. Их содержимое на сервер не отправляется.

3. Как используются данные
• Чтобы давать ежедневные шаги и вести прогресс без сети.
• Чтобы синхронизировать прогресс, когда вы вошли в аккаунт.
• Чтобы подтверждать сессию и отправлять запрошенные коды сброса пароля.
• Чтобы понимать, как пользуются приложением, и поддерживать его работу.

Мы не продаём персональные данные, не используем их для рекламы и не отслеживаем вас в чужих приложениях и на сайтах.

4. Сервисы
Данные получают только обработчики функций, которыми вы пользуетесь:
• Railway размещает сервер и базу.
• Resend отправляет письма для сброса пароля (адрес и одноразовый код).
Они действуют по нашим указаниям и обрабатывают данные на условиях, сопоставимых с этой политикой.

5. Хранение и удаление
• Локальные данные остаются на устройстве, пока вы не очистите данные или не удалите приложение.
• Данные облачного аккаунта хранятся, пока аккаунт существует.
• Удаление аккаунта в Настройках безвозвратно удаляет аккаунт, синхронизированные дни и шаги, события и токены, после чего выполняется выход.
• Журналы сервера хранятся только ограниченное время у хостера и не восстанавливают удалённый аккаунт.

6. Ваш выбор и права
• Приложением можно пользоваться без аккаунта.
• Напоминания включаются и выключаются в Настройках или в настройках устройства.
• Из аккаунта можно выйти в любой момент.
• Аккаунт можно удалить в Настройках. Если сервер не сможет завершить удаление, приложение сообщит об этом; локальный выход и запрос на почту остаются доступны.
• Данные облачного аккаунта можно экспортировать из Настроек в JSON.
• Доступ, исправление или удаление данных можно запросить по эл. почте. В зависимости от страны у вас могут быть и другие права, включая жалобу в орган по защите данных.

7. Дети
Приложение не предназначено для детей младше 13 лет (или другого минимального возраста в вашей стране). Мы сознательно не собираем их данные. Напишите нам, если считаете, что ребёнок передал персональные данные.

8. Международная передача
Поставщики могут обрабатывать данные в других странах на своих условиях и с применимыми гарантиями.

9. Изменения
Политика может обновляться. Дата «Последнее обновление» меняется вместе с ней, а существенные изменения сообщаются в приложении или в карточке магазина.

10. Контакт
Дмитрий Чубрик — supportimproveyourself@gmail.com
""";

    private const string TermsRu = """
Последнее обновление: 24 июля 2026

Эти условия регулируют использование Improve Yourself («приложение»). Пользуясь приложением, вы соглашаетесь с ними.

1. Сервис
Приложение даёт ежедневные шаги и учёт прогресса для личной работы над уверенностью. Оно работает без сети. Облачная синхронизация необязательна и требует аккаунта и интернета.

2. Аккаунты
• Нужен действующий адрес эл. почты, а данные входа нужно хранить в тайне.
• Вы отвечаете за действия в своём аккаунте.
• Аккаунт могут ограничить при злоупотреблении сервисом или попытке несанкционированного доступа.

3. Допустимое использование
Нельзя взламывать, нарушать работу или неправильно использовать приложение и сервер и пытаться получить данные других людей.

4. Ограничение советов
Приложение даёт только общие задания для самостоятельной практики. Это не медицинская, психологическая или терапевтическая помощь. Если нужна профессиональная помощь, обратитесь к специалисту.

5. Интеллектуальная собственность
Приложение, оформление и структура содержания принадлежат разработчику. Права на личные данные, которые вы вводите, остаются у вас.

6. Оплата
Текущие версии могут быть бесплатными. Если позже появятся платные функции, цена и условия продления будут показаны в магазине и на экране покупки до оплаты.

7. Конфиденциальность
Использование также регулируется политикой конфиденциальности в приложении и по публичному адресу политики.

8. Прекращение
Приложением можно перестать пользоваться в любой момент. Аккаунт удаляется в Настройках. Успешное удаление на сервере убирает облачный аккаунт и выполняет выход. Если удаление на сервере не завершилось, приложение сообщит об этом, а локальный выход останется доступен.

9. Отказ от гарантий
Приложение предоставляется «как есть», без гарантий в максимальной степени, допустимой законом.

10. Ограничение ответственности
В максимальной степени, допустимой законом, разработчик не отвечает за косвенные, случайные или последующие убытки из-за использования приложения.

11. Изменения
Условия могут обновляться. Продолжение использования после изменений означает согласие с новой редакцией.

12. Контакт
Вопросы по условиям: Дмитрий Чубрик — supportimproveyourself@gmail.com.
""";

    private const string PrivacyDe = """
Zuletzt aktualisiert: 25. September 2026

Improve Yourself („wir“, „die App“) hilft dir, tägliche Gewohnheiten für sicheres Auftreten aufzubauen. Diese Erklärung beschreibt, welche Daten die App verarbeitet, warum, und wer sie sonst verarbeitet.

1. Verantwortlicher
Betrieben wird die App von Dzmitry Chubryk. Fragen zu Daten und Support: supportimproveyourself@gmail.com.

2. Welche Daten verarbeitet werden
• Kontodaten: E-Mail-Adresse, Passwort-Hash (das Passwort selbst wird nicht gespeichert), Konto-Kennung und Anmelde-Tokens.
• App-Inhalte: Anzeigename, Fortschritt, erledigte Schritte, Antworten der Selbsteinschätzung und Einstellungen.
• Nutzungsereignisse: eine begrenzte Liste von Ereignissen in der App (zum Beispiel „Tag abgeschlossen“) mit Version, Plattform und Gerätetyp. Sie werden nur nach der Anmeldung gesendet und dem Konto zugeordnet.
• Betriebsdaten: IP-Adresse, Zeitpunkt, Route, Antwortstatus und Fehlerdiagnose in Serverprotokollen.
• Lokale Speicherung: Fortschritt und Einstellungen liegen auf dem Gerät; Tokens liegen im sicheren Speicher des Systems.

Erinnerungen werden auf dem Gerät geplant. Ihr Inhalt geht nicht an den Server.

3. Wozu die Daten dienen
• Tägliche Schritte und Fortschritt ohne Netz bereitzustellen.
• Fortschritt zu synchronisieren, wenn du angemeldet bist.
• Die Sitzung zu bestätigen und angeforderte Codes zum Zurücksetzen des Passworts zu senden.
• Zu verstehen, wie die App genutzt wird, und sie zuverlässig zu halten.

Wir verkaufen keine personenbezogenen Daten, nutzen sie nicht für Werbung und verfolgen dich nicht über fremde Apps oder Websites hinweg.

4. Dienste
Daten erhalten nur Verarbeiter der Funktionen, die du nutzt:
• Railway betreibt Server und Datenbank.
• Resend sendet E-Mails zum Zurücksetzen des Passworts (Adresse und Einmalcode).
Sie handeln auf unsere Weisung und zu Bedingungen, die mit dieser Erklärung vergleichbar sind.

5. Speicherung und Löschung
• Lokale Daten bleiben auf dem Gerät, bis du die App-Daten löschst oder die App entfernst.
• Cloud-Daten bleiben, solange das Konto besteht.
• Konto löschen in den Einstellungen entfernt Konto, synchronisierte Tage und Schritte, Ereignisse und Tokens dauerhaft und meldet dich ab.
• Serverprotokolle bleiben nur für die begrenzte Frist des Hosters und stellen kein gelöschtes Konto wieder her.

6. Deine Wahl und Rechte
• Die App kann ohne Konto genutzt werden.
• Erinnerungen lassen sich in den Einstellungen oder in den Geräteeinstellungen ein- und ausschalten.
• Du kannst dich jederzeit abmelden.
• Das Konto lässt sich in den Einstellungen löschen. Wenn der Server die Löschung nicht abschließen kann, sagt die App das; lokale Abmeldung und eine E-Mail-Anfrage bleiben möglich.
• Cloud-Daten lassen sich in den Einstellungen als JSON exportieren.
• Auskunft, Berichtigung oder Löschung kannst du per E-Mail verlangen. Je nach Wohnort hast du weitere Rechte, einschließlich einer Beschwerde bei einer Aufsichtsbehörde.

7. Kinder
Die App richtet sich nicht an Kinder unter 13 Jahren (oder das in deinem Land geltende Mindestalter). Wir erheben ihre Daten nicht wissentlich. Schreib uns, wenn du glaubst, dass ein Kind personenbezogene Daten übermittelt hat.

8. Internationale Übermittlung
Dienstleister können Daten in anderen Ländern verarbeiten, nach ihren Bedingungen und mit den geltenden Schutzmaßnahmen.

9. Änderungen
Diese Erklärung kann aktualisiert werden. Das Datum „Zuletzt aktualisiert“ ändert sich mit, und wesentliche Änderungen werden in der App oder im Store-Eintrag mitgeteilt.

10. Kontakt
Dzmitry Chubryk — supportimproveyourself@gmail.com
""";

    private const string TermsDe = """
Zuletzt aktualisiert: 24. Juli 2026

Diese Nutzungsbedingungen gelten für Improve Yourself („die App“). Mit der Nutzung stimmst du ihnen zu.

1. Der Dienst
Die App bietet tägliche Schritte und Fortschrittsverfolgung zur persönlichen Übung. Sie funktioniert ohne Netz. Die optionale Cloud-Synchronisierung braucht ein Konto und eine Internetverbindung.

2. Konten
• Du brauchst eine gültige E-Mail-Adresse und hältst deine Zugangsdaten geheim.
• Du bist für Aktivitäten unter deinem Konto verantwortlich.
• Konten können eingeschränkt werden, wenn der Dienst missbraucht oder unbefugt auf Daten zugegriffen wird.

3. Zulässige Nutzung
Du darfst die App und den Server nicht zurückentwickeln, stören oder missbrauchen und nicht auf Daten anderer Personen zugreifen.

4. Kein fachlicher Rat
Die App gibt nur allgemeine Übungen zur Selbstentwicklung. Sie ist keine medizinische, psychologische oder therapeutische Beratung. Wenn du fachliche Hilfe brauchst, wende dich an eine qualifizierte Stelle.

5. Geistiges Eigentum
App, Gestaltung und Inhaltsstruktur gehören dem Entwickler. Rechte an den persönlichen Inhalten, die du eingibst, bleiben bei dir.

6. Preise
Aktuelle Versionen können kostenlos sein. Wenn später kostenpflichtige Funktionen kommen, werden Preis und Verlängerung im Store und auf dem Kaufbildschirm gezeigt, bevor du bezahlst.

7. Datenschutz
Die Nutzung unterliegt auch der Datenschutzerklärung in der App und unter der öffentlichen Adresse der Erklärung.

8. Beendigung
Du kannst die App jederzeit nicht mehr nutzen. Das Konto löschst du in den Einstellungen. Eine erfolgreiche Löschung auf dem Server entfernt das Cloud-Konto und meldet dich ab. Wenn die Löschung auf dem Server nicht fertig wird, informiert die App dich, und die lokale Abmeldung bleibt möglich.

9. Keine Gewähr
Die App wird „wie sie ist“ bereitgestellt, ohne Gewähr, soweit das Gesetz das erlaubt.

10. Haftungsbeschränkung
Soweit gesetzlich zulässig, haftet der Entwickler nicht für mittelbare, zufällige oder Folgeschäden aus der Nutzung der App.

11. Änderungen
Die Bedingungen können aktualisiert werden. Weitere Nutzung nach einer Änderung bedeutet, dass du die neue Fassung akzeptierst.

12. Kontakt
Fragen zu diesen Bedingungen: Dzmitry Chubryk — supportimproveyourself@gmail.com.
""";
}
