# treningi
trening-01: 1 płaski teren, 1 pojazd -> l_c1_1
trening-02: 3 płaskie tereny, 1 pojazd -> l_c1_2, l_c1_3, l_c1_4
trening-03: 4 płaskie tereny, 4 pojazdy -> l_c4_1, l_c4_2, l_c4_3, l_c4_4
trening-04: 1 pofalowany teren, 1 pojazd -> h_c1_1
trening-05: 3 pofalowane tereny, 1 pojazd -> h_c1_2, h_c1_3, h_c1_4
trening-06: 4 pofalowane tereny, 4 pojazdy -> h_c4_1, h_c4_2, h_c4_3, h_c4_4


f - (almost) finished
s - stuck
n - slow, not finished

| teren\brain | level1 | level2 | level3 | level4 | level5 | level6 |
| :---------: | :----: | :----: | :----: | :----: | :----: | :----: |
|   l_c4_1    |   3f   |   3f   |   3f   |   3f   |   3f   |   3f   |
|   l_c4_2    |   3s   | 1f 2s  |   3s   | 1f 2s  | 1f 2s  |   3f   |
|   l_c4_3    |   3f   |   3f   | 1f 2s  | 2f 1n  | 1f 2s  |   3f   |
|   l_c4_4    |   3f   |   3f   |   3s   |   3f   | 1f 2s  |   3f   |
|   h_c4_1    |   3s   | 1f 2s  |   3f   | 2f 1n  | 1f 2n  | 1f 2n  |
|   h_c4_2    |   3s   |   3s   |   3s   |   3s   |   3s   |   3s   |
|   h_c4_3    |   3s   |   3s   |   3s   |   3s   |   3s   |   3s   |
|   h_c4_4    | 1f 2s  |   3s   |   3s   |   3s   |   3s   |   3s   |


## Testy
**test-01**
Input 1, oba outputy i obie opcje wyjścia z drogi
Porównanie sprawdza która konfiguracja (output/outside) lepiej działa
**test-03**
Porównanie raysandwalls dla outputu 1 i 2 (outside false)
**test-04**
Porównanie loopdata1, 2 circular i 2 camera na róznych ustawieniach (output 1, outside false)
**test-05**
Porównanie metod input (i1, i6, i7, i8, i9) dla descrete in outside false z poprawionymi kontrolkami pojazdu. 4 instancje na raz, każdy na innej trasie (czarnobiałej)

https://github.com/miyamotok0105/unity-ml-agents/blob/master/docs/Training-PPO.md
**test-06** gamma 0.99
**test-07** gamma 0.80
lepiej 0.80

**test-07** lambd 0.95
**test-08** lambd 0.85
lepiej 0.85

**test-08** buffer_size: 10240
**test-09** buffer_size: 40960
lepiej 10240

**test-08** batch_size: 1024
**test-10** batch_size: 256
**test-11** batch_size: 32
lepiej 256

**test-10** learning_rate: 0.0003
**test-12** learning_rate: 0.00001
**test-13** learning_rate: 0.001
lepiej 0.0003

**test-10** beta: 0.005
**test-14** beta: 0.01
**test-15** beta: 0.0001
lepiej 0.005

**test-10** epsilon: 0.2
**test-16** epsilon: 0.4
lepiej 0.2

**test-10** vis_encode_type: simple
**test-17** vis_encode_type: resnet
**test-18** vis_encode_type: match3
**test-19** vis_encode_type: fully_connected
lepiej match3

**test-18** num_layers: 2 hidden_units: 128
**test-20** num_layers: 4 hidden_units: 192
**test-21** num_layers: 2 hidden_units: 192
**test-22** num_layers: 4 hidden_units: 128

num epoch ~ 8 https://www.youtube.com/watch?v=ZKzXAVp8bC8


potem zmiana rozmiaru sieci, wybór sieci cnn i dodanie pamięci

test 23 -> tylko LIDAR, traning 1

* test na tej samej trasie i różnych tłach
* test na czarnobiałym pofalowanym
* test z dodatkowymi playerami

## Wersje
ml-agents: 0.27.0,
ml-agents-envs: 0.27.0,
Communicator API: 1.5.0,
PyTorch: 1.8.1+cu102
Connected to Unity environment with package version 2.1.0-exp.1 and communication version 1.5.0

## Uruchomienie treningu
```
mlagents-learn --run-id <id> --force --initialize-from <id>
```

## Analiza wyników
```
cd ml-agents/
tensorboard --logdir results/ --port 6006
```
[Opis danych >>>](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Using-Tensorboard.md#the-ml-agents-toolkit-training-statistics)

