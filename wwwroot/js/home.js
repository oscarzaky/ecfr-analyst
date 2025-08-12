$(document).ready(function () {
    const selectedAgency = $("#agencySelect").val();
    const titleSelect = $('#titleSelect');

    if (selectedAgency) {
        $("#titleSelect").parent().show();
    } else {
        $("#titleSelect").empty();
    }

    $('#agencySelect').on('change', function () {
        const agencyId = $(this).val();
        
        if (agencyId) {
            // Show loading spinner
            titleSelect.html('<option value="">Loading titles...</option>').prop('disabled', true);
            
            $.ajax({
                url: `/api/agency/${agencyId}/titles`,
                method: 'GET',
                success: function (data) {
                    console.log('[data]', data);
                    titleSelect.html('<option value="">-- Please select a title --</option>');
                    data.forEach(function (title) {
                        titleSelect.append(`<option value="${title.id}">Title ${title.title}, Chapter ${title.chapter}</option>`);
                    });
                },
                error: function () {
                    titleSelect.html('<option value="">-- Error loading titles --</option>');
                },
                complete: function () {
                    titleSelect.prop('disabled', false);
                }
            });
        } else {
            titleSelect.html('<option value="">-- Please select a title --</option>').prop('disabled', false);
        }
    });

    $('#analyzeBtn').on('click', function () {
        const titleId = $('#titleSelect').val();
        if (!titleId) {
            alert('Please select a title first.');
            return;
        }

        $('#resultsTable').hide();
        $('#resultsError').hide();
        $('#resultsSpinner').show();

        $.ajax({
            url: `/api/analysis/${titleId}`,
            method: 'GET',
            success: function (data) {
                console.log('[analysis data]', data);
                
                // Display corrections
                var correctionsHtml = '';
                if (data.corrections && data.corrections.length > 0) {
                    var correctionCount = data.corrections.length;
                    correctionsHtml += "<h4>Corrections (" + correctionCount + ")</h4>";
                    data.corrections.forEach(function(correction) {
                        console.log('[correction]', correction);
                        correctionsHtml += `<div class="correction-item">
                        <strong>${new Date(correction.last_modified).toLocaleString()}</strong>: ${correction.corrective_action} (${correction.fr_citation})
                    </div>`;
                    });
                } else {
                    correctionsHtml = '<div>No corrections found</div>';
                }
                $('#resTitleNum').text(data.titleNumber);
                $('#resWordCount').text(data.totalWordCount.toLocaleString());
                $('#resCorrectiveHistory').html(correctionsHtml);
                $('#resChecksum').text(data.aggregateChecksum);
                $('#resComplexity').text(data.regulatoryComplexityScore.toFixed(2));
                
                $('#resultsTable').show();
            },
            error: function (jqXHR) {
                $('#resultsError').text('An error occurred: ' + jqXHR.responseText).show();
            },
            complete: function () {
                $('#resultsSpinner').hide();
            }
        });
    });
});