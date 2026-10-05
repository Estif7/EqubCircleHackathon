import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../services/api_service.dart';
import '../theme/app_theme.dart';
import 'circle_details_screen.dart';

class CreateCircleScreen extends StatefulWidget {
  const CreateCircleScreen({super.key});

  @override
  State<CreateCircleScreen> createState() => _CreateCircleScreenState();
}

class _CreateCircleScreenState extends State<CreateCircleScreen> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _amountController = TextEditingController(text: '1000');
  final _limitController = TextEditingController(text: '3');

  String _frequency = 'Monthly';
  DateTime? _startDate;
  bool _isLoading = false;
  String? _errorMessage;

  @override
  void dispose() {
    _nameController.dispose();
    _amountController.dispose();
    _limitController.dispose();
    super.dispose();
  }

  Future<void> _pickDate() async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: _startDate ?? now,
      firstDate: now.subtract(const Duration(days: 1)),
      lastDate: now.add(const Duration(days: 365)),
    );
    if (picked != null) {
      setState(() => _startDate = picked);
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    final name = _nameController.text.trim();
    final amount = double.tryParse(_amountController.text.trim()) ?? 0.0;
    final limit = int.tryParse(_limitController.text.trim()) ?? 3;

    if (amount <= 0) {
      setState(() => _errorMessage = 'Contribution amount must be greater than zero');
      return;
    }

    if (limit < 2) {
      setState(() => _errorMessage = 'Member limit must be at least 2');
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final api = context.read<ApiService>();

    try {
      final circle = await api.createCircle(
        name: name,
        contributionAmount: amount,
        frequency: _frequency,
        memberLimit: limit,
        startDate: _startDate,
      );

      if (mounted) {
        // Navigate directly to circle details
        Navigator.pushReplacement(
          context,
          MaterialPageRoute(
            builder: (_) => CircleDetailsScreen(circleId: circle.id),
          ),
        );
      }
    } catch (e) {
      setState(() {
        _errorMessage = e.toString();
      });
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Create Circle'),
      ),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 480),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppTheme.primaryLight.withValues(alpha: 0.08),
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: AppTheme.primary.withValues(alpha: 0.2)),
                    ),
                    child: const Row(
                      children: [
                        Icon(Icons.info_outline, color: AppTheme.primary, size: 24),
                        SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            'As the creator, you automatically become the organizer and the first member of this circle.',
                            style: TextStyle(
                              fontSize: 13,
                              color: AppTheme.textPrimary,
                              height: 1.3,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 20),

                  if (_errorMessage != null) ...[
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.red.shade50,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.red.shade200),
                      ),
                      child: Text(
                        _errorMessage!,
                        style: TextStyle(color: Colors.red.shade800, fontSize: 13),
                      ),
                    ),
                    const SizedBox(height: 16),
                  ],

                  Form(
                    key: _formKey,
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        TextFormField(
                          controller: _nameController,
                          decoration: const InputDecoration(
                            labelText: 'Circle Name',
                            hintText: 'e.g. Family Savings, Bole Ekub',
                            prefixIcon: Icon(Icons.group_work_outlined),
                          ),
                          validator: (v) =>
                              (v == null || v.trim().isEmpty) ? 'Please enter circle name' : null,
                        ),
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _amountController,
                          keyboardType: const TextInputType.numberWithOptions(decimal: true),
                          decoration: const InputDecoration(
                            labelText: 'Contribution Amount (ETB)',
                            prefixIcon: Icon(Icons.monetization_on_outlined),
                          ),
                          validator: (v) =>
                              (v == null || v.trim().isEmpty) ? 'Enter amount' : null,
                        ),
                        const SizedBox(height: 16),
                        DropdownButtonFormField<String>(
                          initialValue: _frequency,
                          decoration: const InputDecoration(
                            labelText: 'Frequency',
                            prefixIcon: Icon(Icons.calendar_today_outlined),
                          ),
                          items: const [
                            DropdownMenuItem(value: 'Monthly', child: Text('Monthly')),
                            DropdownMenuItem(value: 'Weekly', child: Text('Weekly')),
                          ],
                          onChanged: (val) {
                            if (val != null) setState(() => _frequency = val);
                          },
                        ),
                        const SizedBox(height: 16),
                        TextFormField(
                          controller: _limitController,
                          keyboardType: TextInputType.number,
                          decoration: const InputDecoration(
                            labelText: 'Member Limit',
                            hintText: 'Number of members (e.g. 3, 5, 12)',
                            prefixIcon: Icon(Icons.people_outline),
                          ),
                          validator: (v) =>
                              (v == null || v.trim().isEmpty) ? 'Enter member limit' : null,
                        ),
                        const SizedBox(height: 16),
                        InkWell(
                          onTap: _pickDate,
                          borderRadius: BorderRadius.circular(12),
                          child: InputDecorator(
                            decoration: const InputDecoration(
                              labelText: 'Start Date (Optional)',
                              prefixIcon: Icon(Icons.event_outlined),
                            ),
                            child: Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  _startDate != null
                                      ? DateFormat('MMMM d, yyyy').format(_startDate!)
                                      : 'Select start date',
                                  style: TextStyle(
                                    color: _startDate != null
                                        ? AppTheme.textPrimary
                                        : AppTheme.textMuted,
                                  ),
                                ),
                                if (_startDate != null)
                                  IconButton(
                                    icon: const Icon(Icons.clear, size: 18),
                                    onPressed: () => setState(() => _startDate = null),
                                    padding: EdgeInsets.zero,
                                    constraints: const BoxConstraints(),
                                  )
                              ],
                            ),
                          ),
                        ),
                        const SizedBox(height: 28),
                        ElevatedButton(
                          onPressed: _isLoading ? null : _submit,
                          style: ElevatedButton.styleFrom(minimumSize: const Size.fromHeight(50)),
                          child: _isLoading
                              ? const SizedBox(
                                  height: 20,
                                  width: 20,
                                  child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                                )
                              : const Text('Create Circle'),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
