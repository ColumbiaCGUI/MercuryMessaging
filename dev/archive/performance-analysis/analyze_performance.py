#!/usr/bin/env python3
"""
Performance Analysis Script for MercuryMessaging Framework

Reads CSV data from performance tests and generates:
- Performance graphs (5 graphs as specified in plan)
- Statistical summaries
- Performance report data

Requirements:
    pip install pandas matplotlib numpy

Usage:
    python analyze_performance.py
"""

import os
import sys
from pathlib import Path
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from datetime import datetime


class PerformanceAnalyzer:
    """Analyzes performance test results and generates graphs."""

    def __init__(self, data_dir=None, output_dir=None):
        """
        Initialize analyzer.

        Args:
            data_dir: Directory containing CSV files (default: current directory)
            output_dir: Directory for output graphs (default: graphs/)
        """
        self.data_dir = Path(data_dir or ".").resolve()
        self.output_dir = Path(output_dir or "graphs").resolve()
        self.output_dir.mkdir(parents=True, exist_ok=True)

        print(f"Data directory: {self.data_dir}")
        print(f"Output directory: {self.output_dir}")

        # Data containers
        self.smallscale_data = None
        self.mediumscale_data = None
        self.largescale_data = None
        self.invocation_data = None

        # Statistics
        self.stats = {}

    def load_data(self):
        """Load all CSV files."""
        print("Loading CSV data...")

        # Load test scene results
        files = {
            'smallscale': 'smallscale_results.csv',
            'mediumscale': 'mediumscale_results.csv',
            'largescale': 'largescale_results.csv',
            'invocation': 'invocation_comparison.csv'
        }

        for key, filename in files.items():
            filepath = self.data_dir / filename
            if filepath.exists():
                try:
                    df = pd.read_csv(filepath)
                    setattr(self, f"{key}_data", df)
                    print(f"  ✓ Loaded {filename}: {len(df)} rows")
                except Exception as e:
                    print(f"  ✗ Error loading {filename}: {e}")
            else:
                print(f"  ! {filename} not found (skipping)")

    def calculate_statistics(self):
        """Calculate statistical summaries."""
        print("\nCalculating statistics...")

        datasets = {
            'SmallScale': self.smallscale_data,
            'MediumScale': self.mediumscale_data,
            'LargeScale': self.largescale_data
        }

        for name, data in datasets.items():
            if data is None:
                continue

            stats = {
                'frame_time_ms': {
                    'mean': data['frame_time_ms'].mean(),
                    'std': data['frame_time_ms'].std(),
                    'min': data['frame_time_ms'].min(),
                    'max': data['frame_time_ms'].max(),
                    'p95': data['frame_time_ms'].quantile(0.95)
                },
                'memory_mb': {
                    'mean': data['memory_mb'].mean(),
                    'std': data['memory_mb'].std(),
                    'min': data['memory_mb'].min(),
                    'max': data['memory_mb'].max(),
                    'growth': data['memory_mb'].iloc[-1] - data['memory_mb'].iloc[0]
                },
                'throughput_msg_sec': {
                    'mean': data['throughput_msg_sec'].mean(),
                    'std': data['throughput_msg_sec'].std(),
                    'min': data['throughput_msg_sec'].min(),
                    'max': data['throughput_msg_sec'].max()
                },
                'cache_hit_rate': {
                    'mean': data['cache_hit_rate'].mean() * 100,  # Convert to percentage
                    'std': data['cache_hit_rate'].std() * 100,
                    'min': data['cache_hit_rate'].min() * 100,
                    'max': data['cache_hit_rate'].max() * 100
                }
            }

            self.stats[name] = stats
            print(f"\n{name} Statistics:")
            print(f"  Frame Time: {stats['frame_time_ms']['mean']:.2f}ms ± {stats['frame_time_ms']['std']:.2f}ms")
            print(f"  Memory: {stats['memory_mb']['mean']:.2f}MB (growth: {stats['memory_mb']['growth']:.2f}MB)")
            print(f"  Throughput: {stats['throughput_msg_sec']['mean']:.1f} msg/sec")
            print(f"  Cache Hit Rate: {stats['cache_hit_rate']['mean']:.1f}%")

        # Save statistics to CSV
        stats_df = self._create_stats_dataframe()
        stats_path = self.data_dir / 'performance_statistics_summary.csv'
        stats_df.to_csv(stats_path, index=False)
        print(f"\n✓ Statistics saved to: {stats_path}")

    def _create_stats_dataframe(self):
        """Create DataFrame from statistics."""
        rows = []
        for scene_name, scene_stats in self.stats.items():
            for metric, values in scene_stats.items():
                rows.append({
                    'scene': scene_name,
                    'metric': metric,
                    'mean': values['mean'],
                    'std': values['std'],
                    'min': values['min'],
                    'max': values['max'],
                    'p95': values.get('p95', ''),
                    'growth': values.get('growth', '')
                })
        return pd.DataFrame(rows)

    def _save_figure(self, filename):
        """Helper method to save matplotlib figure with error handling."""
        output_file = self.output_dir / filename

        # Ensure parent directory exists
        output_file.parent.mkdir(parents=True, exist_ok=True)

        # Save with error handling
        try:
            plt.savefig(str(output_file), dpi=300, bbox_inches='tight')
            plt.close()
            if output_file.exists():
                size = output_file.stat().st_size
                print(f"  ✓ {filename} saved to {output_file} ({size:,} bytes)")
                return True
            else:
                print(f"  ✗ WARNING: File save appeared to succeed but file not found at {output_file}")
                return False
        except Exception as e:
            print(f"  ✗ Error saving {filename}: {e}")
            plt.close()
            return False

    def generate_graphs(self):
        """Generate all performance graphs."""
        print("\nGenerating graphs...")
        print(f"Graph output directory: {self.output_dir}")
        print(f"Directory exists: {self.output_dir.exists()}")
        print(f"Directory is writable: {os.access(self.output_dir, os.W_OK)}")

        # Set matplotlib style
        try:
            plt.style.use('seaborn-v0_8-darkgrid')
        except Exception as e:
            print(f"Warning: Could not set plot style: {e}")
            print("Continuing with default style...")

        # Graph 1: Scaling Curves
        try:
            self._graph_scaling_curves()
        except Exception as e:
            print(f"Error generating scaling curves: {e}")

        # Graph 2: Memory Stability
        try:
            self._graph_memory_stability()
        except Exception as e:
            print(f"Error generating memory stability graph: {e}")

        # Graph 3: Cache Effectiveness
        try:
            self._graph_cache_effectiveness()
        except Exception as e:
            print(f"Error generating cache effectiveness graph: {e}")

        # Graph 4: Message Throughput vs Hierarchy Depth
        try:
            self._graph_throughput_vs_depth()
        except Exception as e:
            print(f"Error generating throughput graph: {e}")

        # Graph 5: Frame Time Distribution
        try:
            self._graph_frame_time_distribution()
        except Exception as e:
            print(f"Error generating frame time distribution: {e}")

        print(f"\n✓ Graph generation complete. Output directory: {self.output_dir}")
        print("\nVerifying saved files:")
        for graph_file in ['scaling_curves.png', 'memory_stability.png', 'cache_effectiveness.png',
                          'throughput_vs_depth.png', 'frame_time_distribution.png']:
            full_path = self.output_dir / graph_file
            if full_path.exists():
                size = full_path.stat().st_size
                print(f"  ✓ {graph_file} ({size:,} bytes) at {full_path}")
            else:
                print(f"  ✗ {graph_file} NOT FOUND")

    def _graph_scaling_curves(self):
        """Graph 1: Frame Time vs Load (Scaling Curves)."""
        fig, ax = plt.subplots(figsize=(12, 6))

        datasets = {
            'Small (10 resp, 3 lvl)': self.smallscale_data,
            'Medium (50 resp, 5 lvl)': self.mediumscale_data,
            'Large (100+ resp, 7-10 lvl)': self.largescale_data
        }

        for label, data in datasets.items():
            if data is not None:
                # Use timestamp as x-axis to show progression
                ax.plot(data['timestamp'], data['frame_time_ms'],
                       label=label, linewidth=2, alpha=0.7)

        ax.set_xlabel('Time (seconds)', fontsize=12)
        ax.set_ylabel('Frame Time (ms)', fontsize=12)
        ax.set_title('Frame Time Scaling Across Different Loads', fontsize=14, fontweight='bold')
        ax.legend(loc='best', fontsize=10)
        ax.grid(True, alpha=0.3)
        ax.axhline(y=16.6, color='r', linestyle='--', label='60 FPS Target', alpha=0.5)

        plt.tight_layout()
        self._save_figure('scaling_curves.png')

    def _graph_memory_stability(self):
        """Graph 2: Memory Usage Over Time."""
        fig, ax = plt.subplots(figsize=(12, 6))

        datasets = {
            'Small': self.smallscale_data,
            'Medium': self.mediumscale_data,
            'Large': self.largescale_data
        }

        for label, data in datasets.items():
            if data is not None:
                ax.plot(data['messages_sent'], data['memory_mb'],
                       label=f'{label} Scale', linewidth=2, alpha=0.7)

        ax.set_xlabel('Messages Sent (count)', fontsize=12)
        ax.set_ylabel('Memory Usage (MB)', fontsize=12)
        ax.set_title('Memory Stability Over Message Volume (QW-4 CircularBuffer)',
                    fontsize=14, fontweight='bold')
        ax.legend(loc='best', fontsize=10)
        ax.grid(True, alpha=0.3)

        plt.tight_layout()
        self._save_figure('memory_stability.png')

    def _graph_cache_effectiveness(self):
        """Graph 3: Cache Hit Rate vs Responder Count."""
        fig, ax = plt.subplots(figsize=(12, 6))

        # Aggregate cache hit rates over time for each scene
        datasets = {
            'Small (10 responders)': self.smallscale_data,
            'Medium (50 responders)': self.mediumscale_data,
            'Large (100+ responders)': self.largescale_data
        }

        responder_counts = []
        cache_hit_rates = []

        for label, data in datasets.items():
            if data is not None:
                # Extract responder count from label
                count_str = label.split('(')[1].split(' ')[0]
                if '+' in count_str:
                    count = 100
                else:
                    count = int(count_str)

                responder_counts.append(count)
                # Use mean cache hit rate, convert to percentage
                cache_hit_rates.append(data['cache_hit_rate'].mean() * 100)

        if responder_counts:
            ax.plot(responder_counts, cache_hit_rates,
                   marker='o', markersize=10, linewidth=2, color='#2E86AB')
            ax.scatter(responder_counts, cache_hit_rates, s=100, color='#A23B72', zorder=5)

            ax.set_xlabel('Number of Responders', fontsize=12)
            ax.set_ylabel('Cache Hit Rate (%)', fontsize=12)
            ax.set_title('Filter Cache Effectiveness (QW-3)', fontsize=14, fontweight='bold')
            ax.grid(True, alpha=0.3)
            ax.set_ylim(0, 100)

            # Add target line
            ax.axhline(y=80, color='g', linestyle='--', label='Target: 80%', alpha=0.5)
            ax.legend(loc='best', fontsize=10)

        plt.tight_layout()
        self._save_figure('cache_effectiveness.png')

    def _graph_throughput_vs_depth(self):
        """Graph 4: Message Throughput vs Hierarchy Depth."""
        fig, ax = plt.subplots(figsize=(12, 6))

        depths = [3, 5, 10]  # Approximate depths for each scene
        labels = ['Small', 'Medium', 'Large']
        datasets = [self.smallscale_data, self.mediumscale_data, self.largescale_data]

        throughputs = []
        for data in datasets:
            if data is not None:
                throughputs.append(data['throughput_msg_sec'].mean())
            else:
                throughputs.append(0)

        # Filter out missing data
        valid_data = [(d, t, l) for d, t, l in zip(depths, throughputs, labels) if t > 0]
        if valid_data:
            depths, throughputs, labels = zip(*valid_data)

            colors = ['#06A77D', '#F5853F', '#D62246'][:len(depths)]
            bars = ax.bar(labels, throughputs, color=colors, alpha=0.7, edgecolor='black', linewidth=1.5)

            # Add value labels on bars
            for bar, throughput in zip(bars, throughputs):
                height = bar.get_height()
                ax.text(bar.get_x() + bar.get_width()/2., height,
                       f'{throughput:.1f}',
                       ha='center', va='bottom', fontsize=11, fontweight='bold')

            ax.set_xlabel('Scene (Hierarchy Depth)', fontsize=12)
            ax.set_ylabel('Message Throughput (msg/sec)', fontsize=12)
            ax.set_title('Sustained Message Throughput vs Hierarchy Complexity',
                        fontsize=14, fontweight='bold')
            ax.grid(True, alpha=0.3, axis='y')

        plt.tight_layout()
        self._save_figure('throughput_vs_depth.png')

    def _graph_frame_time_distribution(self):
        """Graph 5: Frame Time Distribution (Histogram)."""
        fig, axes = plt.subplots(1, 3, figsize=(18, 5))

        datasets = {
            'Small': self.smallscale_data,
            'Medium': self.mediumscale_data,
            'Large': self.largescale_data
        }

        for idx, (label, data) in enumerate(datasets.items()):
            ax = axes[idx]
            if data is not None:
                ax.hist(data['frame_time_ms'], bins=50, color='#5FA8D3',
                       alpha=0.7, edgecolor='black', linewidth=0.5)

                # Add mean line
                mean_val = data['frame_time_ms'].mean()
                ax.axvline(mean_val, color='r', linestyle='--', linewidth=2,
                          label=f'Mean: {mean_val:.2f}ms')

                # Add 60 FPS target line
                ax.axvline(16.6, color='g', linestyle='--', linewidth=2,
                          label='60 FPS Target', alpha=0.7)

                ax.set_xlabel('Frame Time (ms)', fontsize=11)
                ax.set_ylabel('Frequency', fontsize=11)
                ax.set_title(f'{label} Scale', fontsize=12, fontweight='bold')
                ax.legend(loc='best', fontsize=9)
                ax.grid(True, alpha=0.3, axis='y')
            else:
                ax.text(0.5, 0.5, 'No Data', ha='center', va='center',
                       transform=ax.transAxes, fontsize=14)
                ax.set_title(f'{label} Scale', fontsize=12, fontweight='bold')

        plt.suptitle('Frame Time Distribution Across Scales', fontsize=14, fontweight='bold', y=1.02)
        plt.tight_layout()
        self._save_figure('frame_time_distribution.png')

    def generate_invocation_comparison_graph(self):
        """Generate graph for InvocationComparison results."""
        if self.invocation_data is None:
            print("\n! InvocationComparison data not available, skipping graph")
            return

        print("\nGenerating InvocationComparison graph...")

        fig, ax = plt.subplots(figsize=(12, 6))

        test_types = self.invocation_data['test_type'].tolist()
        avg_ms = self.invocation_data['avg_ms'].tolist()

        colors = ['#1D3557', '#457B9D', '#A8DADC', '#F1FAEE', '#E63946'][:len(test_types)]
        bars = ax.bar(test_types, avg_ms, color=colors, alpha=0.8, edgecolor='black', linewidth=1.5)

        # Add value labels
        for bar, val in zip(bars, avg_ms):
            height = bar.get_height()
            ax.text(bar.get_x() + bar.get_width()/2., height,
                   f'{val:.4f}ms',
                   ha='center', va='bottom', fontsize=10, fontweight='bold')

        ax.set_xlabel('Invocation Method', fontsize=12)
        ax.set_ylabel('Average Time (ms)', fontsize=12)
        ax.set_title('Performance Comparison: MercuryMessaging vs Unity Built-in Systems',
                    fontsize=14, fontweight='bold')
        ax.grid(True, alpha=0.3, axis='y')

        plt.tight_layout()
        self._save_figure('invocation_comparison.png')

    def run_full_analysis(self):
        """Run complete analysis pipeline."""
        print("=" * 60)
        print("MercuryMessaging Performance Analysis")
        print("=" * 60)

        self.load_data()
        self.calculate_statistics()
        self.generate_graphs()
        self.generate_invocation_comparison_graph()

        print("\n" + "=" * 60)
        print("Analysis Complete!")
        print("=" * 60)
        print(f"\nResults:")
        print(f"  • Graphs: {self.output_dir}")
        print(f"  • Statistics CSV: {self.data_dir / 'performance_statistics_summary.csv'}")
        print("\nNext steps:")
        print("  1. Review graphs in graphs/ folder")
        print("  2. Check statistics CSV for numerical data")
        print("  3. Generate performance report (see PERFORMANCE_REPORT.md template)")


def main():
    """Main entry point."""
    # Check for required packages
    try:
        import pandas
        import matplotlib
        import numpy
    except ImportError as e:
        print(f"Error: Missing required package: {e}")
        print("\nInstall requirements with:")
        print("  pip install pandas matplotlib numpy")
        sys.exit(1)

    # Run analysis
    analyzer = PerformanceAnalyzer()
    analyzer.run_full_analysis()


if __name__ == '__main__':
    main()
