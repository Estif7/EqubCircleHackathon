import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/models.dart';
import '../services/api_service.dart';
import '../theme/app_theme.dart';
import '../widgets/common_widgets.dart';
import 'circle_details_screen.dart';
import 'create_circle_screen.dart';

class BrowseCirclesScreen extends StatefulWidget {
  const BrowseCirclesScreen({super.key});

  @override
  State<BrowseCirclesScreen> createState() => _BrowseCirclesScreenState();
}

class _BrowseCirclesScreenState extends State<BrowseCirclesScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final TextEditingController _searchController = TextEditingController();

  List<CircleSummary> _available = [];
  List<CircleSummary> _mine = [];
  bool _isLoading = true;
  String _frequency = '';
  String _search = '';
  int? _joiningId;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _load();
  }

  @override
  void dispose() {
    _tabController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() => _isLoading = true);
    final api = context.read<ApiService>();

    try {
      final availableF = api.getAvailableCircles(search: _search, frequency: _frequency);
      final mineF = api.getMyCircles();
      final res = await Future.wait([availableF, mineF]);

      if (mounted) {
        setState(() {
          _available = res[0];
          _mine = res[1];
          _isLoading = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _join(int id) async {
    setState(() => _joiningId = id);
    final api = context.read<ApiService>();
    try {
      await api.joinCircle(id);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Joined circle successfully!')),
        );
        _load();
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Failed: $e'), backgroundColor: Colors.red),
        );
      }
    } finally {
      if (mounted) setState(() => _joiningId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Circles'),
        bottom: TabBar(
          controller: _tabController,
          labelColor: AppTheme.primary,
          unselectedLabelColor: AppTheme.textSecondary,
          indicatorColor: AppTheme.primary,
          tabs: [
            Tab(text: 'Available (${_available.length})'),
            Tab(text: 'My Circles (${_mine.length})'),
          ],
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh_rounded),
            onPressed: _load,
          ),
          IconButton(
            icon: const Icon(Icons.add_circle_outline_rounded),
            onPressed: () async {
              final created = await Navigator.push<bool>(
                context,
                MaterialPageRoute(builder: (_) => const CreateCircleScreen()),
              );
              if (created == true) _load();
            },
          ),
        ],
      ),
      body: Column(
        children: [
          // Filter Bar
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _searchController,
                    decoration: InputDecoration(
                      hintText: 'Search circles…',
                      prefixIcon: const Icon(Icons.search, size: 20),
                      suffixIcon: _search.isNotEmpty
                          ? IconButton(
                              icon: const Icon(Icons.clear, size: 18),
                              onPressed: () {
                                _searchController.clear();
                                setState(() => _search = '');
                                _load();
                              },
                            )
                          : null,
                      isDense: true,
                      contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                    ),
                    onSubmitted: (v) {
                      setState(() => _search = v.trim());
                      _load();
                    },
                  ),
                ),
                const SizedBox(width: 8),
                PopupMenuButton<String>(
                  initialValue: _frequency,
                  onSelected: (val) {
                    setState(() => _frequency = val);
                    _load();
                  },
                  itemBuilder: (context) => const [
                    PopupMenuItem(value: '', child: Text('All frequencies')),
                    PopupMenuItem(value: 'Monthly', child: Text('Monthly')),
                    PopupMenuItem(value: 'Weekly', child: Text('Weekly')),
                  ],
                  child: Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 12),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppTheme.border),
                    ),
                    child: Row(
                      children: [
                        Icon(
                          Icons.filter_list_rounded,
                          size: 18,
                          color: _frequency.isNotEmpty ? AppTheme.primary : AppTheme.textSecondary,
                        ),
                        const SizedBox(width: 4),
                        Text(
                          _frequency.isEmpty ? 'All' : _frequency,
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                            color: _frequency.isNotEmpty ? AppTheme.primary : AppTheme.textSecondary,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),

          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : TabBarView(
                    controller: _tabController,
                    children: [
                      _buildAvailableList(),
                      _buildMineList(),
                    ],
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildAvailableList() {
    if (_available.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.search_off_rounded, size: 48, color: Colors.grey.shade400),
            const SizedBox(height: 12),
            const Text(
              'No available circles found',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppTheme.textPrimary),
            ),
            const SizedBox(height: 4),
            const Text('Try modifying your search or filter.', style: TextStyle(color: AppTheme.textMuted)),
          ],
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      itemCount: _available.length,
      separatorBuilder: (context, index) => const SizedBox(height: 10),
      itemBuilder: (context, index) {
        final c = _available[index];
        final isJoining = _joiningId == c.id;

        return Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const StatusBadge(status: 'Open'),
                    Text(
                      c.frequency,
                      style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 12, color: AppTheme.textSecondary),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                Text(
                  c.name,
                  style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: AppTheme.textPrimary),
                ),
                const SizedBox(height: 2),
                Text('Organized by ${c.organizerName}', style: const TextStyle(fontSize: 12, color: AppTheme.textMuted)),
                const SizedBox(height: 12),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '${currencyFormat.format(c.contributionAmount)} ETB',
                          style: const TextStyle(fontSize: 16, fontWeight: FontWeight.w800, color: AppTheme.primary),
                        ),
                        Text('${c.memberCount} of ${c.memberLimit} joined',
                            style: const TextStyle(fontSize: 11, color: AppTheme.textSecondary)),
                      ],
                    ),
                    ElevatedButton(
                      onPressed: isJoining ? null : () => _join(c.id),
                      child: isJoining
                          ? const SizedBox(
                              width: 16,
                              height: 16,
                              child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                            )
                          : const Text('Join Circle'),
                    ),
                  ],
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildMineList() {
    if (_mine.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.donut_large_outlined, size: 48, color: Colors.grey.shade400),
            const SizedBox(height: 12),
            const Text(
              'You have not joined any circles yet',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: AppTheme.textPrimary),
            ),
            const SizedBox(height: 4),
            const Text('Join an available circle or create your own!', style: TextStyle(color: AppTheme.textMuted)),
          ],
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      itemCount: _mine.length,
      separatorBuilder: (context, index) => const SizedBox(height: 10),
      itemBuilder: (context, index) {
        final c = _mine[index];
        return Card(
          child: InkWell(
            borderRadius: BorderRadius.circular(16),
            onTap: () {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder: (_) => CircleDetailsScreen(circleId: c.id),
                ),
              ).then((_) => _load());
            },
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      StatusBadge(status: c.status),
                      Text(
                        c.frequency,
                        style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 12, color: AppTheme.textSecondary),
                      ),
                    ],
                  ),
                  const SizedBox(height: 8),
                  Text(
                    c.name,
                    style: const TextStyle(fontSize: 17, fontWeight: FontWeight.bold, color: AppTheme.textPrimary),
                  ),
                  const SizedBox(height: 2),
                  Text('Organized by ${c.organizerName}', style: const TextStyle(fontSize: 12, color: AppTheme.textMuted)),
                  const Divider(height: 20),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Text('Contribution', style: TextStyle(fontSize: 10, color: AppTheme.textMuted)),
                          Text(
                            '${currencyFormat.format(c.contributionAmount)} ETB',
                            style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700, color: AppTheme.primary),
                          ),
                        ],
                      ),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.end,
                        children: [
                          const Text('Members', style: TextStyle(fontSize: 10, color: AppTheme.textMuted)),
                          Text(
                            '${c.memberCount} / ${c.memberLimit}',
                            style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w700, color: AppTheme.textPrimary),
                          ),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
        );
      },
    );
  }
}
