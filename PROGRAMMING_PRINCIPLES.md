1. DRY (Don't Repeat Yourself)
Принцип реалізовано шляхом винесення логіки відтворення звуків у окремий клас ProcessCom. Замість того, щоб ініціалізувати аудіопристрої в кожному методі, використовується приватний метод PlaySound.

    Приклад у коді: [main/ProcessCom.cs#L7C1-L23C6](https://github.com/VanyaBond/Assistant-Astra/blob/main/main/ProcessCom.cs#L7C1-L23C6) (метод PlaySound).

2. SRP (Single Responsibility Principle)
Кожен клас відповідає за свою вузьку задачу:

    VolumeControl — виключно керування рівнем гучності системи: [main/VolumeControl.cs](https://github.com/VanyaBond/Assistant-Astra/blob/main/main/VolumeControl.cs)
    
    ProcessCom — взаємодія з користувачем через звукові ефекти: [main/ProcessCom.cs](https://github.com/VanyaBond/Assistant-Astra/blob/main/main/ProcessCom.cs)
    

3. Separation of Concerns (Розподіл обов'язків)
Логіка розпізнавання мови (Vosk/Porcupine) відокремлена від логіки виконання команд. Основний цикл обробки в Program.cs лише викликає ProcessCommand.

    Приклад у коді: [main/Program.cs#L72C7-L175C10](https://github.com/VanyaBond/Assistant-Astra/blob/main/main/Program.cs#L72C7-L175C10)
