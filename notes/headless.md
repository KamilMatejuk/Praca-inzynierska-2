# steps taken
sudo apt-get install xvfb
touch /etc/init.d/xvfb
chmod +x /etc/init.d/xvfb

pip install ipython


https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Executable.md


mlagents-learn train_configuration.yaml --env=envs/training-v1 --run-id=test-24 --initialize-from test-23



trainings:
|  id   |         trasy         |                               | instancje | pojazdy na tor |      tekstury       | iteracje |
| :---: | :-------------------: | :---------------------------: | :-------: | :------------: | :-----------------: | :------: |
|   1   | jednakowy tor - okrąg |           c1_bw_l_0           |    18     |       1        |     czarnobiale     |  6 mln   |
|   2   |    3 podobne tory     | c1_bw_l_1,c1_bw_l_2,c1_bw_l_3 |   po 6    |       1        |     czarnobiałe     |  6 mln   |
|   3   |    3 podobne tory     |  c1_f_l_0,c1_d_l_0,c1_m_l_0   |   po 6    |       1        | las, pustynia, góry |  6 mln   |
|   4   |  1 skąplikowany tor   |           c1_bw_l_4           |    18     |       1        |     czarnobiale     |  6 mln   |
|   5   |   3 pofalowane tory   | c1_bw_h_0,c1_bw_h_1,c1_bw_h_2 |   po 6    |       1        |     czarnobiałe     |  6 mln   |
|   6   |    3 podobne tory     | c4_bw_l_0,c4_bw_l_1,c4_bw_l_2 |   po 2    |       4        |     czarnobiałe     |  6 mln   |
|   7   |    3 podobne tory     |  c4_f_l_0,c4_d_l_0,c4_m_l_0   |   po 2    |       4        | las, pustynia, góry |  6 mln   |
|   8   |   3 pofalowane tory   |  c4_f_h_0,c4_d_h_0,c4_m_h_0   |   po 2    |       4        | las, pustynia, góry |  6 mln   |

c1_bw_l_0 -> 1 car, blackwhite, low
c2_f_h_0 -> 2 cars, forest, high
c2_d_h_0 -> 2 cars, desert, high
c2_m_h_0 -> 2 cars, mountains, high
