export interface DashboardStats {
  totalUsers: number;
  totalVendors: number;
  totalProducts: number;
  totalOrders: number;
  totalRevenue: number;
  pendingVendors: number;
  activeUsers: number;
  lowStockProducts: number;
}

export interface RevenuePoint {
  month: string;
  revenue: number;
  orders: number;
}

export interface ChartData {
  labels: string[];
  datasets: {
    label: string;
    data: number[];
    backgroundColor?: string | string[];
    borderColor?: string;
    fill?: boolean;
  }[];
}

export interface SystemHealth {
  cpu: number;
  memory: number;
  disk: number;
  uptime: string;
  status: 'healthy' | 'warning' | 'critical';
}

export interface CmsPage {
  id: number;
  title: string;
  slug: string;
  content: string;
  metaTitle: string;
  metaDescription: string;
  isPublished: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PayoutRequest {
  id: number;
  payoutNumber?: string;
  vendorId: number;
  vendorName: string;
  storeName: string;
  amount: number;
  fee: number;
  netAmount: number;
  status: 'pending' | 'approved' | 'processing' | 'completed' | 'rejected';
  periodStart: string;
  periodEnd: string;
  requestedAt: string;
  processedAt?: string;
  notes?: string;
}

export interface AnalyticsMetric {
  label: string;
  value: number;
  previousValue: number;
  change: number;
  trend: 'up' | 'down' | 'stable';
}

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  filterable?: boolean;
  type?: 'text' | 'number' | 'date' | 'currency' | 'badge' | 'action';
  width?: string;
  format?: (value: any) => string;
}

export interface TableAction {
  label: string;
  icon: string;
  class?: string;
  handler: (row: any) => void;
  visible?: (row: any) => boolean;
}

export interface FilterOption {
  key: string;
  label: string;
  type: 'text' | 'select' | 'date' | 'number';
  options?: { label: string; value: any }[];
  placeholder?: string;
}
