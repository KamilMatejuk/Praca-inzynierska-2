import csv
import random
from perlin_noise import PerlinNoise
from math import log2, log10, sqrt
import matplotlib.pyplot as plt

SMOOTHING = 0.9


def smooth(scalars):  # Weight between 0 and 1
    last = scalars[0]
    smoothed = list()
    for point in scalars:
        smoothed_val = last * SMOOTHING + (1 - SMOOTHING) * point
        smoothed.append(smoothed_val)
        last = smoothed_val
    return smoothed


def get_data_from_file(filename: str):
    data = []
    with open(filename) as f:
        for row in csv.DictReader(f):
            data.append((int(row['Step']), float(row['Value'])))
    return data


def generate_graph(names, data_reward, data_checkpoints, filename, minr=None, maxr=None, minc=None, maxc=None):
    colors = ['#CC3311', '#228148', '#9883E5', '#FF6B6B', '#00FF00']
    plt.clf()
    plt.cla()
    plt.close()
    
    # Cut your window in 1 row and 2 columns, and start a plot in the first part
    plt.subplot(121)
    plt.title('Cumulative Reward')
    plt.xlabel('Training step')
    plt.xticks([0, 5000000, 10000000, 15000000, 20000000], ['0', '5M', '10M', '15M', '20M'])
    for i, d in enumerate(data_reward):
        steps = [i[0] for i in d]
        values = [i[1]/2-150 for i in d]
        import copy
        v = copy.copy(values)
        v.sort()
        print("reward")
        print(f"średnia: {sum(values) / len(values)}")
        print(f"top 99%: {sum(v[int(0.01 * len(v)):]) / len(v[int(0.01 * len(v)):])}")
        print(f"top 90%: {sum(v[int(0.1 * len(v)):]) / len(v[int(0.1 * len(v)):])}")
        print(f"top 50%: {sum(v[int(0.5 * len(v)):]) / len(v[int(0.5 * len(v)):])}")
        print(f"max: {v[-1]}")
        values_smoothend = smooth(values)
        c = colors[i]
        n = names[i]
        # if n == 'Dane odległości od krawędzi':
        #     values = [v/1.5-2 for v in values]
        #     values_smoothend = smooth(values)
        plt.plot(steps, values, color=f"{c}55")
        plt.plot(steps, values_smoothend, color=f"{c}ff", label=n)
    if (minr is not None and maxr is not None):
        plt.ylim(minr, maxr)
    
    # Cut your window in 1 row and 2 columns, and start a plot in the second part
    lines = []
    plt.subplot(122)
    plt.title('Visited Checkpoints')
    plt.xlabel('Training step')
    plt.xticks([0, 5000000, 10000000, 15000000, 20000000], ['0', '5M', '10M', '15M', '20M'])
    for i, d in enumerate(data_checkpoints):
        steps = [i[0] for i in d]
        values = [i[1]/2 for i in d]
        import copy
        v = copy.copy(values)
        v.sort()
        print("checkpoints")
        print(f"średnia: {sum(values) / len(values)}")
        print(f"top 99%: {sum(v[int(0.01 * len(v)):]) / len(v[int(0.01 * len(v)):])}")
        print(f"top 90%: {sum(v[int(0.1 * len(v)):]) / len(v[int(0.1 * len(v)):])}")
        print(f"top 50%: {sum(v[int(0.5 * len(v)):]) / len(v[int(0.5 * len(v)):])}")
        print(f"max: {v[-1]}")
        values_smoothend = smooth(values)
        c = colors[i]
        n = names[i]
        plt.plot(steps, values, color=f"{c}55")
        l, = plt.plot(steps, values_smoothend, color=f"{c}ff", label=n)
        lines.append(l)
    if (minc is not None and maxc is not None):
        plt.ylim(minc, maxc)
    
    plt.figlegend(lines, names, bbox_to_anchor=[0.5, 0], loc='lower center', ncol=2)
    fig = plt.gcf()
    fig.set_size_inches(10, 4)
    plt.tight_layout()
    plt.subplots_adjust(bottom=0.2)
    plt.savefig(filename)
    # plt.show()
    
def example_graph(filename, minr=None, maxr=None, minc=None, maxc=None):
    plt.clf()
    plt.cla()
    plt.close()
    
    # Cut your window in 1 row and 2 columns, and start a plot in the first part
    noise = PerlinNoise()
    plt.subplot(121)
    plt.title('Cumulative Reward')
    plt.xlabel('Training step')
    plt.xticks([0, 10000000, 20000000, 30000000, 40000000, 50000000], ['0', '10M', '20M', '30M', '40M', '50M'])    
    steps = [i*5000 for i in range(1, 1000)]
    values = [log2(i+250000) for i in steps]
    values = [v + 2*noise(v*15+500000) for v in values]
    values_smoothend = smooth(values)
    plt.plot(steps, values, color="#CC331155")
    plt.plot(steps, values_smoothend, color="#CC3311ff")
    # plt.ylim(minc, maxc)
    
    # Cut your window in 1 row and 2 columns, and start a plot in the second part
    noise = PerlinNoise()
    plt.subplot(122)
    plt.title('Visited Checkpoints')
    plt.xlabel('Training step')
    plt.xticks([0, 10000000, 20000000, 30000000, 40000000, 50000000], ['0', '10M', '20M', '30M', '40M', '50M'])    
    steps = [i*5000 for i in range(1, 1000)]
    values = [log10(i+250000) for i in steps]
    values = [v + noise(v*10) for v in values]
    values_smoothend = smooth(values)
    plt.plot(steps, values, color="#CC331155")
    plt.plot(steps, values_smoothend, color="#CC3311ff")
    # plt.ylim(minc, maxc)
    
    fig = plt.gcf()
    fig.set_size_inches(10, 4)
    plt.tight_layout()
    plt.subplots_adjust(bottom=0.15)
    plt.savefig(filename)
    # plt.show()


if __name__ == '__main__':
    # #################### gamma ####################
    # names = ['gamma: 0.99', 'gamma: 0.80']
    # data_reward = [
    #     get_data_from_file('run-test-06_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-07_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-06_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-07_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_gamma-2.png', -5, 6, 0.5, 1.9)
    
    # #################### lambd ####################
    # names = ['lambd: 0.95', 'lambd: 0.85']
    # data_reward = [
    #     get_data_from_file('run-test-07_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-08_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-07_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-08_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_lambd-2.png', -3, 6, 0.5, 1.9)

    # #################### buffer_size ####################
    # names = ['buffer_size: 10240', 'buffer_size: 40960']
    # data_reward = [
    #     get_data_from_file('run-test-08_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-09_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-08_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-09_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_buffer_size-2.png', -5, 6, 0.5, 1.9)

    # #################### batch_size ####################
    # names = ['batch_size: 1024', 'batch_size: 256', 'batch_size: 32']
    # data_reward = [
    #     get_data_from_file('run-test-08_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-10_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-11_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-08_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-10_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-11_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_batch_size-2.png', -3, 6, 0.5, 1.9)
    
    # #################### learning_rate ####################
    # names = ['learning_rate: 0.00001', 'learning_rate: 0.0003', 'learning_rate: 0.001']
    # data_reward = [
    #     get_data_from_file('run-test-12_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-10_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-13_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-12_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-10_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-13_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_learning_rate-2.png', -6, 5.5, 0.3, 1.9)
    
    # #################### beta ####################
    # names = ['beta: 0.0001', 'beta: 0.005', 'beta: 0.01']
    # data_reward = [
    #     get_data_from_file('run-test-15_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-10_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-14_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-15_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-10_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-14_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_beta-2.png', -2, 6, 0.5, 1.9)
    
    #################### epsilon ####################
    # names = ['epsilon: 0.2', 'epsilon: 0.4']
    # data_reward = [
    #     get_data_from_file('run-test-10_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-16_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-10_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-16_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_epsilon-2.png', -3, 6, 0.5, 1.9)

    # #################### vis_encode_type ####################
    # names = [
    #     'vis_encode_type: simple',
    #     'vis_encode_type: resnet',
    #     'vis_encode_type: match3',
    #     'vis_encode_type: fully_connected',
    # ]
    # data_reward = [
    #     get_data_from_file('run-test-10_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-17_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-18_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-19_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    # ]
    # data_checkpoints = [
    #     get_data_from_file('run-test-10_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-17_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-18_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-19_in9-out1-f-tag-VisitedCheckpoints.csv'),
    # ]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_vis_encode_type-2.png', -3, 5.5, 0.5, 1.9)
    
    #################### network size ####################
    # names = ['num_layers: 2 hidden_units: 128', 'num_layers: 4 hidden_units: 192']
    # data_reward = [
    #     get_data_from_file('run-test-18_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-20_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-18_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-20_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_network_size-2.png', -3, 5.5, 0.5, 1.9)

    # #################### select input ####################
    # names = [
    #     'Dane wynikające z trasy',
    #     'Dane odległości od krawędzi',
    #     'Dane wizualne jednowymiarowe',
    #     'Dane wizualne z kamery przedniej',
    #     'Dane wizualne z kamery z lotu ptaka',
    # ]
    # data_reward = [
    #     get_data_from_file('run-test-05_in1-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-05_in5-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-05_in7-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-05_in8-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-05_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    # ]
    # data_checkpoints = [
    #     get_data_from_file('run-test-05_in1-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-05_in5-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-05_in7-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-05_in8-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-05_in9-out1-f-tag-VisitedCheckpoints.csv'),
    # ]
    # generate_graph(names, data_reward, data_checkpoints, 'input_observations.png', -3, 13, 1, 6)
    
    #################### hyperparameters tuning results ####################
    # names = ['przed', 'po']
    # data_reward = [
    #     get_data_from_file('run-test-06_in9-out1-f-tag-Environment_Cumulative Reward.csv'),
    #     get_data_from_file('run-test-18_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [
    #     get_data_from_file('run-test-06_in9-out1-f-tag-VisitedCheckpoints.csv'),
    #     get_data_from_file('run-test-18_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'hyperparameters_tuning_results-2.png', -5, 6, 0.5, 2)
    
    #################### traning lvl 1 ####################
    # lidar + camera top
    # names = ['etap 1']
    # data_reward = [get_data_from_file('run-test-00_in9-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-test-00_in9-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_1.png', 0, 35, 0.5, 8)
    # only lidar
    # names = ['etap 1']
    # data_reward = [get_data_from_file('run-test-23_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-test-23_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_1.png', 0, 220, 0, 11)
    
    ################### traning lvl 1 ####################
    # names = ['etap 1']
    # data_reward = [get_data_from_file('run-level1_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level1_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_1.png', -13, 13, 3, 4.7)
    
    ################### traning lvl 2 ####################
    # names = ['etap 2']
    # data_reward = [get_data_from_file('run-level2_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level2_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_2.png', -19, -13, 1.6, 2.2)
    
    ################### traning lvl 3 ####################
    # names = ['etap 3']
    # data_reward = [get_data_from_file('run-level3_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level3_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_3.png', -25, -7, 1.3, 3.3)
    
    ################### traning lvl 4 ####################
    # names = ['etap 4']
    # data_reward = [get_data_from_file('run-level4_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level4_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_4.png', -120, 0, 1.6, 6)
    
    ################### traning lvl 5 ####################
    # names = ['etap 5']
    # data_reward = [get_data_from_file('run-level5_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level5_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_5.png', -43, -25, 1.2, 1.75)
    
    ################### traning lvl 6 ####################
    # names = ['etap 6']
    # data_reward = [get_data_from_file('run-level6_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level6_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_6.png', -26, -13, 0.52, 0.95)
    
    ################### traning lvl 6_3 ####################
    names = ['etap 6']
    data_reward = [get_data_from_file('run-level6_3_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    data_checkpoints = [get_data_from_file('run-level6_3_in5-out1-f-tag-VisitedCheckpoints.csv')]
    generate_graph(names, data_reward, data_checkpoints, 'training_progress_6.png', -50, 120, 5, 17)
    
    ################### traning lvl 7 ####################
    # names = ['etap 7']
    # data_reward = [get_data_from_file('run-level7_in5-out1-f-tag-Environment_Cumulative Reward.csv')]
    # data_checkpoints = [get_data_from_file('run-level7_in5-out1-f-tag-VisitedCheckpoints.csv')]
    # generate_graph(names, data_reward, data_checkpoints, 'training_progress_7.png', -30, -16, 0.6, 0.86)
    
    # example_graph('example.png')
    pass
