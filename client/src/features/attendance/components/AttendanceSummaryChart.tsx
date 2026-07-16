import { useMemo } from 'react';
import ReactECharts from 'echarts-for-react';
import type { AttendanceDaySummary } from '../api';

// Fixed status colors (never themed as categorical hues) — see the dataviz skill's
// status palette. Severity maps naturally onto attendance meaning:
// OnTime = good, Late = warning, ExcusedAbsence = serious, Absent = critical.
const STATUS_COLORS = {
  onTime: '#0ca30c',
  late: '#fab219',
  excusedAbsence: '#ec835a',
  absent: '#d03b3b',
};

interface AttendanceSummaryChartProps {
  data: AttendanceDaySummary[];
}

export function AttendanceSummaryChart({ data }: AttendanceSummaryChartProps) {
  const isDark =
    typeof window !== 'undefined' && window.matchMedia('(prefers-color-scheme: dark)').matches;

  const inkSecondary = isDark ? '#c3c2b7' : '#52514e';
  const gridline = isDark ? '#2c2c2a' : '#e1e0d9';
  const baseline = isDark ? '#383835' : '#c3c2b7';

  const option = useMemo(
    () => ({
      backgroundColor: 'transparent',
      textStyle: { color: inkSecondary, fontFamily: 'system-ui, -apple-system, "Segoe UI", sans-serif' },
      legend: {
        top: 0,
        left: 0,
        textStyle: { color: inkSecondary },
        data: ['On time', 'Late', 'Excused absence', 'Absent'],
      },
      grid: { left: 40, right: 16, top: 40, bottom: 32 },
      tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
      xAxis: {
        type: 'category',
        data: data.map((d) => d.date),
        axisLine: { lineStyle: { color: baseline } },
        axisLabel: { color: inkSecondary },
        axisTick: { show: false },
      },
      yAxis: {
        type: 'value',
        minInterval: 1,
        splitLine: { lineStyle: { color: gridline } },
        axisLabel: { color: inkSecondary },
      },
      series: [
        {
          name: 'On time',
          type: 'bar',
          stack: 'total',
          barWidth: 18,
          itemStyle: { color: STATUS_COLORS.onTime },
          data: data.map((d) => d.onTimeCount),
        },
        {
          name: 'Late',
          type: 'bar',
          stack: 'total',
          barWidth: 18,
          itemStyle: { color: STATUS_COLORS.late },
          data: data.map((d) => d.lateCount),
        },
        {
          name: 'Excused absence',
          type: 'bar',
          stack: 'total',
          barWidth: 18,
          itemStyle: { color: STATUS_COLORS.excusedAbsence },
          data: data.map((d) => d.excusedAbsenceCount),
        },
        {
          name: 'Absent',
          type: 'bar',
          stack: 'total',
          barWidth: 18,
          itemStyle: { color: STATUS_COLORS.absent },
          data: data.map((d) => d.absentCount),
        },
      ],
    }),
    [data, inkSecondary, gridline, baseline],
  );

  if (data.length === 0) {
    return <p className="text-slate-500 dark:text-slate-400">No attendance recorded in this range yet.</p>;
  }

  return <ReactECharts option={option} style={{ height: 320 }} notMerge />;
}
